namespace DatabaseBackupTool.ConsoleApp.Models;

/// <summary>
/// The location to store the backup.
/// </summary>
public enum BackupLocation
{
    Local = 0,
    AzureBlobStorage = 1
}