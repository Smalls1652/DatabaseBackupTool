using System.ComponentModel;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Options;

namespace DatabaseBackupTool.ConsoleApp.Services;

public sealed class KeyVaultService : IKeyVaultService, IDisposable
{
    private bool _disposed;

    private readonly SecretClient _secretClient;

    public KeyVaultService(IOptions<KeyVaultServiceOptions> options)
    {
        _secretClient = new(
            vaultUri: new($"https://{options.Value.KeyVaultName}.vault.azure.net/"),
            credential: new ChainedTokenCredential([
                new AzureCliCredential(),
                new AzurePowerShellCredential(),
                new ManagedIdentityCredential()
            ])
        );
    }

    public KeyVaultService(KeyVaultServiceOptions options)
    {
        _secretClient = new(
            vaultUri: new($"https://{options.KeyVaultName}.vault.azure.net/"),
            credential: new ChainedTokenCredential([
                new AzureCliCredential(),
                new AzurePowerShellCredential(),
                new ManagedIdentityCredential()
            ])
        );
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        KeyVaultSecret secret = await _secretClient.GetSecretAsync(secretName);

        return secret.Value;
    }

    public void Dispose()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        _disposed = true;

        GC.SuppressFinalize(this);
    }
}
