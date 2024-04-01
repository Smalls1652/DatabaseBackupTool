namespace DatabaseBackupTool.ConsoleApp.DatabaseProviders;

public interface IDatabaseProvider
{
    Task DumpDatabaseAsync(string outputPath, CancellationToken cancellationToken = default);

    Task<string> CompressDumpAsync(string outputPath, CancellationToken cancellationToken = default);
}