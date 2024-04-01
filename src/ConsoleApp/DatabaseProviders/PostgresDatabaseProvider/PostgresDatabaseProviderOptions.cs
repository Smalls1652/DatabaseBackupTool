namespace DatabaseBackupTool.ConsoleApp.DatabaseProviders;

/// <summary>
/// Options for the <see cref="PostgresDatabaseProvider" />.
/// </summary>
public sealed class PostgresDatabaseProviderOptions
{
    /// <summary>
    /// The host of the Postgres database.
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// The port of the Postgres database.
    /// </summary>
    public int Port { get; set; } = 5432;

    /// <summary>
    /// The username to connect to the Postgres database.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The password to connect to the Postgres database.
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// The name of the Postgres database.
    /// </summary>
    public string Database { get; set; } = null!;
}