using Azure.Identity;
using Azure.Storage.Blobs;
using DatabaseBackupTool.ConsoleApp.Models;

namespace DatabaseBackupTool.ConsoleApp.BackupProviders;

/// <summary>
/// Backup provider for Azure Blob Storage.
/// </summary>
public sealed class AzureStorageBlobProvider : IBackupProvider
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly AzureBlobStorageConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureStorageBlobProvider"/> class.
    /// </summary>
    /// <param name="config">Configuration options for Azure Blob Storage.</param>
    public AzureStorageBlobProvider(AzureBlobStorageConfig config)
    {
        _config = config;

        _blobServiceClient = new(
            serviceUri: config.EndpointUri,
            credential: new ChainedTokenCredential(
                [
                    new AzureCliCredential(),
                    new AzurePowerShellCredential(),
                    new ManagedIdentityCredential()
                ]
            )
        );
    }

    /// <inheritdoc />
    public async Task UploadBackupAsync(string backupFilePath, CancellationToken cancellationToken)
    {
        BlobContainerClient containerClient = _blobServiceClient.GetBlobContainerClient(_config.ContainerName);

        using FileStream fileStream = File.OpenRead(backupFilePath);

        await containerClient.UploadBlobAsync(
            blobName: Path.GetFileName(backupFilePath),
            content: fileStream,
            cancellationToken: cancellationToken
        );
    }
}