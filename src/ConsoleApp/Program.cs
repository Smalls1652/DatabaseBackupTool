using DatabaseBackupTool.ConsoleApp;
using DatabaseBackupTool.ConsoleApp.DatabaseProviders;
using DatabaseBackupTool.ConsoleApp.Extensions;
using DatabaseBackupTool.ConsoleApp.Models;
using DatabaseBackupTool.ConsoleApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddSlimHostLifetime();

builder.Configuration
    .AddJsonFile(
        path: "appsettings.json",
        optional: true,
        reloadOnChange: true
    )
    .AddJsonFile(
        path: $"appsettings.{builder.Environment}.json",
        optional: true,
        reloadOnChange: true
    )
    .AddEnvironmentVariables()
    .AddCommandLine(
        args: args,
        switchMappings: RootCommandLineMappings.SwitchMappings
    );

if (builder.Configuration.GetValue<bool>("USE_KEY_VAULT"))
{

    KeyVaultService kvService = new(
        options: new KeyVaultServiceOptions()
        {
            KeyVaultName = builder.Configuration.GetValue<string>("KEY_VAULT_NAME") ?? throw new NullReferenceException("KEY_VAULT_NAME or --key-vault-name is required")
        }
    );

    builder.Configuration["DATABASE_PASSWORD"] = await kvService.GetSecretAsync(
        secretName: builder.Configuration.GetValue<string>("KEY_VAULT_SECRET_DATABASE_PASSWORD") ?? throw new NullReferenceException("KEY_VAULT_SECRET_DATABASE_PASSWORD or --key-vault-secret-database-password is required")
    );

    kvService.Dispose();
}

string? databaseTypeSettingValue = builder.Configuration.GetValue<string>("DATABASE_TYPE");
DatabaseType databaseType = databaseTypeSettingValue is not null
    ? Enum.Parse<DatabaseType>(databaseTypeSettingValue, ignoreCase: true)
    : throw new NullReferenceException("DATABASE_TYPE or --database-type is required");

if (databaseType == DatabaseType.Postgres)
{
    builder.Services
        .AddPostgresDatabaseProvider(options =>
        {
            options.Host = builder.Configuration.GetValue<string>("DATABASE_HOST") ?? throw new NullReferenceException("DATABASE_HOST or --host is required");
            options.Port = builder.Configuration.GetValue<int>("DATABASE_PORT") == 0 ? 5432 : builder.Configuration.GetValue<int>("DATABASE_PORT");
            options.Username = builder.Configuration.GetValue<string>("DATABASE_USERNAME") ?? throw new NullReferenceException("DATABASE_USERNAME or --username is required");
            options.Password = builder.Configuration.GetValue<string>("DATABASE_PASSWORD") ?? throw new NullReferenceException("DATABASE_PASSWORD or --password is required");
            options.Database = builder.Configuration.GetValue<string>("DATABASE_NAME") ?? throw new NullReferenceException("DATABASE_NAME or --database is required");
        });
}
else
{
    throw new Exception("An error occurred while selecting the database provider.");
}

builder.Services
    .AddMainService(
        options =>
        {
            options.OutputPath = builder.Configuration.GetValue<string>("OUTPUT_PATH") ?? throw new NullReferenceException("OUTPUT_PATH or --output-path is required");

            string? backupLocationSettingValue = builder.Configuration.GetValue<string>("BACKUP_LOCATION");
            BackupLocation backupLocation = backupLocationSettingValue is not null
                ? Enum.Parse<BackupLocation>(backupLocationSettingValue, ignoreCase: true)
                : BackupLocation.Local;

            options.BackupLocation = backupLocation;

            if (backupLocation == BackupLocation.AzureBlobStorage)
            {
                options.AzureBlobStorageConfig = new AzureBlobStorageConfig
                {
                    EndpointUri = builder.Configuration.GetValue<Uri>("AZURE_BLOB_STORAGE_ENDPOINT_URI") ?? throw new NullReferenceException("AZURE_BLOB_STORAGE_ENDPOINT_URI is required"),
                    ContainerName = builder.Configuration.GetValue<string>("AZURE_BLOB_STORAGE_CONTAINER_NAME") ?? throw new NullReferenceException("AZURE_BLOB_STORAGE_CONTAINER_NAME is required")
                };
            }
        }
    );

var app = builder.Build();

await app.RunAsync();
