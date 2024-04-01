using DatabaseBackupTool.ConsoleApp.Models;

namespace DatabaseBackupTool.ConsoleApp.Services;

/// <summary>
/// Configuration options for <see cref="MainService"/>. 
/// </summary>
public sealed class MainServiceOptions
{
    /// <summary>
    /// The host of the PostgreSQL server.
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// The port of the PostgreSQL server.
    /// </summary>
    public int Port { get; set; } = 5432;

    /// <summary>
    /// The username to use when connecting to the PostgreSQL server.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The password to use when connecting to the PostgreSQL server.
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// The name of the database to backup.
    /// </summary>
    public string Database { get; set; } = null!;

    /// <summary>
    /// The path to the directory where the backup files will be stored.
    /// </summary>
    public string OutputPath { get; set; } = null!;

    /// <summary>
    /// The location to store the backup.
    /// </summary>
    public BackupLocation BackupLocation { get; set; } = BackupLocation.Local;

    /// <summary>
    /// Configuration for Azure Blob Storage.
    /// </summary>
    public AzureBlobStorageConfig? AzureBlobStorageConfig { get; set; }

    /// <summary>
    /// Gets the resolved output path.
    /// </summary>
    /// <returns>The resolved path to the directory.</returns>
    /// <exception cref="DirectoryNotFoundException"></exception>
    /// <exception cref="IOException"></exception>
    public string GetFullOutputPath()
    {
        if (!Directory.Exists(Path.GetDirectoryName(OutputPath)))
        {
            throw new DirectoryNotFoundException($"The directory '{Path.GetDirectoryName(OutputPath)}' does not exist");
        }

        if (Directory.Exists(OutputPath))
        {
            throw new IOException($"The path '{OutputPath}' already exists. Please specify a different path.");
        }

        return Path.GetFullPath(OutputPath);
    }
}