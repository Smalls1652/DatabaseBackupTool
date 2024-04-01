using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DatabaseBackupTool.ConsoleApp.BackupProviders;
using DatabaseBackupTool.ConsoleApp.Exceptions;
using DatabaseBackupTool.ConsoleApp.Models;

namespace DatabaseBackupTool.ConsoleApp.Services;

/// <summary>
/// The main hosted service.
/// </summary>
public sealed class MainService : IHostedService, IDisposable
{
    private bool _disposed;
    private Task? _executingTask;
    private CancellationTokenSource? _cts;

    private readonly MainServiceOptions _options;
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainService"/> class.
    /// </summary>
    /// <param name="options">The options for the service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="appLifetime">The application lifetime.</param>
    public MainService(IOptions<MainServiceOptions> options, ILogger<MainService> logger, IHostApplicationLifetime appLifetime)
    {
        _options = options.Value;
        _logger = logger;
        _appLifetime = appLifetime;
    }

    /// <summary>
    /// The main task that runs the backup process.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The exit code.</returns>
    public async Task<int> RunAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            string outputPathFull;
            try
            {
                outputPathFull = _options.GetFullOutputPath();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while validating the output path.");

                return 1;
            }

            try
            {
                await DumpDatabaseAsync(cancellationToken);
            }
            catch (PgDumpProcessException ex)
            {
                _logger.LogError(ex, "An error occurred while running the 'pg_dump' process.");

                return 1;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unknown error occurred while dumping the database.");

                return 1;
            }

            string compressedOutputPath;
            try
            {
                compressedOutputPath = await CompressDumpAsync(outputPathFull, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while compressing the backup.");

                return 1;
            }

            if (_options.BackupLocation == BackupLocation.AzureBlobStorage)
            {
                _logger.LogInformation("Uploading backup to Azure Blob Storage...");

                try
                {
                    AzureStorageBlobProvider azureStorageBlobProvider = new(_options.AzureBlobStorageConfig!);

                    await azureStorageBlobProvider.UploadBackupAsync(compressedOutputPath, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while uploading the backup to Azure Blob Storage.");

                    return 1;
                }
            }

            _logger.LogInformation("Backup completed successfully.");

            return 0;
        }
        finally
        {
            _appLifetime.StopApplication();
        }
    }

    /// <summary>
    /// Runs the 'pg_dump' process to dump the database.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    /// <exception cref="PgDumpProcessException"></exception>
    private async Task DumpDatabaseAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Backing up database '{Database}' to '{OutputPath}'...", _options.Database, _options.OutputPath);

        ProcessStartInfo pgdumpStartInfo = new(
            fileName: "pg_dump",
            arguments: [
                "--host",
                $"{_options.Host}",
                "--port",
                $"{_options.Port}",
                "--username",
                $"{_options.Username}",
                "--dbname",
                $"{_options.Database}",
                "--no-password",
                "--format",
                "directory",
                "--file",
                $"{_options.GetFullOutputPath()}"
            ]
        )
        {
            Environment =
            {
                ["PGPASSWORD"] = _options.Password
            },
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        Process pgdumpProcess = new()
        {
            StartInfo = pgdumpStartInfo
        };

        pgdumpProcess.Start();

        await pgdumpProcess.WaitForExitAsync(cancellationToken);

        if (pgdumpProcess.ExitCode != 0)
        {
            string pgdumpProcessError = await pgdumpProcess.StandardError.ReadToEndAsync();

            throw new PgDumpProcessException(pgdumpProcessError);
        }
    }

    /// <summary>
    /// Compresses the backup directory to a '.tar.gz' file.
    /// </summary>
    /// <param name="dirPath">The path to the directory to compress.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    private async Task<string> CompressDumpAsync(string dirPath, CancellationToken cancellationToken)
    {
        DateTimeOffset currentDateTime = DateTimeOffset.Now;

        string outputParentDirectory = Path.GetDirectoryName(dirPath)!;
        string tarOutputPath = Path.Combine(outputParentDirectory, $"{Path.GetFileName(dirPath)}_{currentDateTime:yyyy-MM-dd_HH-mm-ss}.tar");
        string compressedOutputPath = Path.Combine(outputParentDirectory, $"{Path.GetFileName(dirPath)}_{currentDateTime:yyyy-MM-dd_HH-mm-ss}.tar.gz");

        _logger.LogInformation("Compressing backup to '{CompressedOutputPath}'...", compressedOutputPath);

        using FileStream tarOutputStream = File.Create(tarOutputPath);

        TarFile.CreateFromDirectory(
            sourceDirectoryName: dirPath,
            destination: tarOutputStream,
            includeBaseDirectory: true
        );

        Directory.Delete(dirPath, recursive: true);

        tarOutputStream.Position = 0;

        using FileStream compressedOutputFileStream = File.Create(compressedOutputPath);

        using GZipStream gZipStream = new(compressedOutputFileStream, CompressionMode.Compress);

        await tarOutputStream.CopyToAsync(compressedOutputFileStream, cancellationToken);

        tarOutputStream.Close();
        File.Delete(tarOutputPath);

        return compressedOutputPath;
    }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        _executingTask = RunAsync(_cts.Token);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_executingTask is null)
        {
            return;
        }

        try
        {
            _cts?.Cancel();
        }
        finally
        {
            await _executingTask
                .WaitAsync(cancellationToken)
                .ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
        }
    }

    /// <inheritdoc />
    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _executingTask?.Dispose();
        _cts?.Dispose();

        _disposed = true;
        GC.SuppressFinalize(this);
    }
}