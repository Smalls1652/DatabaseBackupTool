namespace DatabaseBackupTool.ConsoleApp.DatabaseProviders;

/// <summary>
/// Interface for database providers.
/// </summary>
public interface IDatabaseProvider
{
    /// <summary>
    /// Dump the database to the specified output path.
    /// </summary>
    /// <param name="outputPath">The path to dump the database to.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task DumpDatabaseAsync(string outputPath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Compress the dump at the specified output path.
    /// </summary>
    /// <param name="outputPath">The path to the dumped database.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The path to the compressed dump.</returns>
    Task<string> CompressDumpAsync(string outputPath, CancellationToken cancellationToken = default);
}