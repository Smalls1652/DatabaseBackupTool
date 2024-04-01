using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using DatabaseBackupTool.ConsoleApp.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DatabaseBackupTool.ConsoleApp.DatabaseProviders;

/// <summary>
/// Database provider for Postgres databases.
/// </summary>
public sealed class PostgresDatabaseProvider : IDatabaseProvider
{
    private readonly PostgresDatabaseProviderOptions _options;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="PostgresDatabaseProvider"/>.
    /// </summary>
    /// <param name="options">The options for the Postgres database provider.</param>
    /// <param name="logger">The logger.</param>
    public PostgresDatabaseProvider(IOptions<PostgresDatabaseProviderOptions> options, ILogger<PostgresDatabaseProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DumpDatabaseAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Dumping Postgres database '{Database}' to '{OutputPath}'...", _options.Database, outputPath);
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
                $"{outputPath}"
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

        using Process pgdumpProcess = new()
        {
            StartInfo = pgdumpStartInfo
        };

        pgdumpProcess.Start();

        await pgdumpProcess.WaitForExitAsync(cancellationToken);

        if (pgdumpProcess.ExitCode != 0)
        {
            string pgdumpProcessError = await pgdumpProcess.StandardError.ReadToEndAsync(cancellationToken);

            throw new DumpProcessException(pgdumpProcessError);
        }
    }

    /// <inheritdoc />
    public async Task<string> CompressDumpAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        DateTimeOffset currentDateTime = DateTimeOffset.Now;

        string outputParentDirectory = Path.GetDirectoryName(outputPath)!;
        string tarOutputPath = Path.Combine(outputParentDirectory, $"{Path.GetFileName(outputPath)}_{currentDateTime:yyyy-MM-dd_HH-mm-ss}.tar");
        string compressedOutputPath = Path.Combine(outputParentDirectory, $"{Path.GetFileName(outputPath)}_{currentDateTime:yyyy-MM-dd_HH-mm-ss}.tar.gz");

        _logger.LogInformation("Compressing backup to '{CompressedOutputPath}'...", compressedOutputPath);

        using FileStream tarOutputStream = File.Create(tarOutputPath);

        TarFile.CreateFromDirectory(
            sourceDirectoryName: outputPath,
            destination: tarOutputStream,
            includeBaseDirectory: true
        );

        Directory.Delete(outputPath, recursive: true);

        tarOutputStream.Position = 0;

        using FileStream compressedOutputFileStream = File.Create(compressedOutputPath);

        using GZipStream gZipStream = new(compressedOutputFileStream, CompressionMode.Compress);

        await tarOutputStream.CopyToAsync(compressedOutputFileStream, cancellationToken);

        tarOutputStream.Close();
        File.Delete(tarOutputPath);

        return compressedOutputPath;
    }
}