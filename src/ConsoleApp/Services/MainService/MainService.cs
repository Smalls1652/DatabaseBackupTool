using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DatabaseBackupTool.ConsoleApp.BackupProviders;
using DatabaseBackupTool.ConsoleApp.Exceptions;
using DatabaseBackupTool.ConsoleApp.Models;
using DatabaseBackupTool.ConsoleApp.DatabaseProviders;

namespace DatabaseBackupTool.ConsoleApp.Services;

/// <summary>
/// The main hosted service.
/// </summary>
public sealed class MainService : IHostedService, IDisposable
{
    private bool _disposed;
    private Task? _executingTask;
    private CancellationTokenSource? _cts;

    private readonly IDatabaseProvider _databaseProvider;
    private readonly MainServiceOptions _options;
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _appLifetime;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainService"/> class.
    /// </summary>
    /// <param name="options">The options for the service.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="appLifetime">The application lifetime.</param>
    public MainService(IDatabaseProvider databaseProvider, IOptions<MainServiceOptions> options, ILogger<MainService> logger, IHostApplicationLifetime appLifetime)
    {
        _databaseProvider = databaseProvider;
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
                await _databaseProvider.DumpDatabaseAsync(outputPathFull, cancellationToken);
            }
            catch (DumpProcessException ex)
            {
                _logger.LogError(ex, "An error occurred while dumping the database.");

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
                compressedOutputPath = await _databaseProvider.CompressDumpAsync(outputPathFull, cancellationToken);
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