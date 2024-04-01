namespace DatabaseBackupTool.ConsoleApp.DatabaseProviders;

/// <summary>
/// Options for the <see cref="MySqlDatabaseProvider" />.
/// </summary>
public sealed class MySqlDatabaseProviderOptions
{
    /// <summary>
    /// The host of the MySQL database.
    /// </summary>
    public string Host { get; set; } = null!;

    /// <summary>
    /// The port of the MySQL database.
    /// </summary>
    public int Port { get; set; } = 3306;

    /// <summary>
    /// The username to connect to the MySQL database.
    /// </summary>
    public string Username { get; set; } = null!;

    /// <summary>
    /// The password to connect to the MySQL database.
    /// </summary>
    public string Password { get; set; } = null!;

    /// <summary>
    /// The name of the MySQL database.
    /// </summary>
    public string Database { get; set; } = null!;
}