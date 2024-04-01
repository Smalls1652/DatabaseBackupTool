using System.Text.Json.Serialization;

namespace DatabaseBackupTool.ConsoleApp.Models;

/// <summary>
/// Configuration for the backup.
/// </summary>
public sealed class BackupAppSettings
{
    /// <summary>
    /// The host for the PostgreSQL server.
    /// </summary>
    [JsonPropertyName("host")]
    public string Host { get; set; } = null!;

    /// <summary>
    /// The port for the PostgreSQL server.
    /// </summary>
    [JsonPropertyName("port")]
    public int Port { get; set; } = 5432;

    /// <summary>
    /// The username to use when connecting to the PostgreSQL server.
    /// </summary>
    [JsonPropertyName("username")]
    public string Username { get; set; } = null!;

    /// <summary>
    /// The password to use when connecting to the PostgreSQL server.
    /// </summary>
    [JsonIgnore]
    public string Password { get; set; } = null!;

    /// <summary>
    /// The name of the database to backup.
    /// </summary>
    [JsonPropertyName("database")]
    public string Database { get; set; } = null!;

    /// <summary>
    /// The path to the directory where the backup files will be stored.
    /// </summary>
    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = null!;
}