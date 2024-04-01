using System.Diagnostics;
using System.Formats.Tar;
using System.IO.Compression;
using DatabaseBackupTool.ConsoleApp.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DatabaseBackupTool.ConsoleApp.DatabaseProviders;

/// <summary>
/// Database provider for MySQL databases.
/// </summary>
public sealed class MySqlDatabaseProvider : IDatabaseProvider
{
    private readonly MySqlDatabaseProviderOptions _options;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of <see cref="MySqlDatabaseProvider"/>.
    /// </summary>
    /// <param name="options">The options for the MySQL database provider.</param>
    /// <param name="logger">The logger.</param>
    public MySqlDatabaseProvider(IOptions<MySqlDatabaseProviderOptions> options, ILogger<MySqlDatabaseProvider> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task DumpDatabaseAsync(string outputPath, CancellationToken cancellationToken = default)
    {
        // In order for the `mysqldump` process to work without a password prompt,
        // we need to create a temporary defaults file with the password.
        // All of the `mysql` tools consider the environment variable method to be "insecure".
        string tempDirectory = Path.Combine(Path.GetTempPath(), "dbbt_mysql");

        if (!Directory.Exists(tempDirectory))
        {
            Directory.CreateDirectory(tempDirectory);
        }

        string defaultsFilePath = Path.Combine(tempDirectory, "my.cnf");
        string defaultsFileContents = $"[client]\npassword={_options.Password}";

        if (File.Exists(defaultsFilePath))
        {
            File.Delete(defaultsFilePath);
        }

        await File.WriteAllTextAsync(defaultsFilePath, defaultsFileContents, cancellationToken);
        
        if (!Directory.Exists(outputPath))
        {
            Directory.CreateDirectory(outputPath);
        }

        string outputFilePath = Path.Combine(outputPath, $"{_options.Database}.sql");

        _logger.LogInformation("Dumping MySQL database '{Database}' to '{OutputPath}'...", _options.Database, outputFilePath);

        try
        {
            ProcessStartInfo mysqldumpStartInfo = new(
                fileName: "mysqldump",
                arguments: [
                    "--host",
                    $"{_options.Host}",
                    "--port",
                    $"{_options.Port}",
                    "--user",
                    $"{_options.Username}",
                    "--databases",
                    $"{_options.Database}",
                    "--result-file",
                    $"{outputFilePath}"
                ]
            )
            {
                Environment =
                {
                    ["MYSQL_HOME"] = tempDirectory
                },
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };

            using Process mysqldumpProcess = new()
            {
                StartInfo = mysqldumpStartInfo
            };

            mysqldumpProcess.Start();

            await mysqldumpProcess.WaitForExitAsync(cancellationToken);

            if (mysqldumpProcess.ExitCode != 0)
            {
                string mysqldumpProcessError = await mysqldumpProcess.StandardError.ReadToEndAsync(cancellationToken);

                Directory.Delete(outputPath, true);

                throw new DumpProcessException(mysqldumpProcessError);
            }
        }
        finally
        {
            // Clean up the temporary defaults file.
            Directory.Delete(tempDirectory, true);
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