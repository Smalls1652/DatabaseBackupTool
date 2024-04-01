namespace DatabaseBackupTool.ConsoleApp.BackupProviders;

/// <summary>
/// Interface for backup providers.
/// </summary>
public interface IBackupProvider
{
    /// <summary>
    /// Uploads a backup to the provider.
    /// </summary>
    /// <param name="backupFilePath">Path to the backup file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    Task UploadBackupAsync(string backupFilePath, CancellationToken cancellationToken);
}