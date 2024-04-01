namespace DatabaseBackupTool.ConsoleApp;

/// <summary>
/// Constants for the root command line mappings.
/// </summary>
public static class RootCommandLineMappings
{
    /// <summary>
    /// Switch mappings for command line arguments.
    /// </summary>
    public static Dictionary<string, string> SwitchMappings = new()
    {
        ["--host"] = "DATABASE_HOST",
        ["--port"] = "DATABASE_PORT",
        ["--username"] = "DATABASE_USERNAME",
        ["--password"] = "DATABASE_PASSWORD",
        ["--database"] = "DATABASE_NAME",
        ["--output-path"] = "OUTPUT_PATH",
        ["--backup-location"] = "BACKUP_LOCATION",
        ["--azure-blob-storage-endpoint-uri"] = "AZURE_BLOB_STORAGE_ENDPOINT_URI",
        ["--azure-blob-storage-container-name"] = "AZURE_BLOB_STORAGE_CONTAINER_NAME",
        ["--use-key-vault"] = "USE_KEY_VAULT",
        ["--key-vault-name"] = "KEY_VAULT_NAME",
        ["--key-vault-secret-database-password"] = "KEY_VAULT_SECRET_DATABASE_PASSWORD"
    };
}