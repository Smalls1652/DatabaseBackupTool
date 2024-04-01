namespace DatabaseBackupTool.ConsoleApp.Models;

/// <summary>
/// Configuration for Azure Blob Storage.
/// </summary>
public sealed class AzureBlobStorageConfig
{
    /// <summary>
    /// The endpoint URI for the Azure Blob Storage account.
    /// </summary>
    public Uri EndpointUri { get; set; } = null!;

    /// <summary>
    /// The name of the container to upload backups to.
    /// </summary>
    public string ContainerName { get; set; } = null!;
}