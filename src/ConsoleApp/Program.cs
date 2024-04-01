using DatabaseBackupTool.ConsoleApp;
using DatabaseBackupTool.ConsoleApp.Extensions;
using DatabaseBackupTool.ConsoleApp.Models;
using DatabaseBackupTool.ConsoleApp.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using ILoggerFactory appLoggerFactory = LoggerFactory.Create(options =>
{
    options.AddSimpleConsole();
});

ILogger appLogger = appLoggerFactory.CreateLogger("DatabaseBackupTool");

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
            KeyVaultName = builder.Configuration.GetValue<string>("KEY_VAULT_NAME") ?? throw new MissingConfigurationValueException("KEY_VAULT_NAME", "--key-vault-name")
        }
    );

    builder.Configuration["DATABASE_PASSWORD"] = await kvService.GetSecretAsync(
        secretName: builder.Configuration.GetValue<string>("KEY_VAULT_SECRET_DATABASE_PASSWORD") ?? throw new MissingConfigurationValueException("KEY_VAULT_SECRET_DATABASE_PASSWORD", "--key-vault-secret-database-password")
    );

    kvService.Dispose();
}


string? databaseTypeSettingValue = builder.Configuration.GetValue<string>("DATABASE_TYPE");
DatabaseType databaseType;

try
{
    databaseType = databaseTypeSettingValue is not null
    ? Enum.Parse<DatabaseType>(databaseTypeSettingValue, ignoreCase: true)
    : throw new MissingConfigurationValueException("DATABASE_TYPE", "--database-type");
}
catch (MissingConfigurationValueException ex)
{
    appLogger.LogError("{Message}", ex.Message);

    return 1;
}

if (databaseType == DatabaseType.Postgres)
{
    builder.Services
        .AddPostgresDatabaseProvider(options =>
        {
            options.Host = builder.Configuration.GetValue<string>("DATABASE_HOST") ?? throw new MissingConfigurationValueException("DATABASE_HOST", "--host");
            options.Port = builder.Configuration.GetValue<int>("DATABASE_PORT") == 0 ? 5432 : builder.Configuration.GetValue<int>("DATABASE_PORT");
            options.Username = builder.Configuration.GetValue<string>("DATABASE_USERNAME") ?? throw new MissingConfigurationValueException("DATABASE_USERNAME", "--username");
            options.Password = builder.Configuration.GetValue<string>("DATABASE_PASSWORD") ?? throw new MissingConfigurationValueException("DATABASE_PASSWORD", "--password");
            options.Database = builder.Configuration.GetValue<string>("DATABASE_NAME") ?? throw new MissingConfigurationValueException("DATABASE_NAME", "--database");
        });
}

builder.Services
    .AddMainService(
        options =>
        {
            options.OutputPath = builder.Configuration.GetValue<string>("OUTPUT_PATH") ?? throw new MissingConfigurationValueException("OUTPUT_PATH", "--output-path");

            string? backupLocationSettingValue = builder.Configuration.GetValue<string>("BACKUP_LOCATION");
            BackupLocation backupLocation = backupLocationSettingValue is not null
                ? Enum.Parse<BackupLocation>(backupLocationSettingValue, ignoreCase: true)
                : BackupLocation.Local;

            options.BackupLocation = backupLocation;

            if (backupLocation == BackupLocation.AzureBlobStorage)
            {
                options.AzureBlobStorageConfig = new AzureBlobStorageConfig
                {
                    EndpointUri = builder.Configuration.GetValue<Uri>("AZURE_BLOB_STORAGE_ENDPOINT_URI") ?? throw new MissingConfigurationValueException("AZURE_BLOB_STORAGE_ENDPOINT_URI", "--azure-blob-storage-endpoint-uri"),
                    ContainerName = builder.Configuration.GetValue<string>("AZURE_BLOB_STORAGE_CONTAINER_NAME") ?? throw new MissingConfigurationValueException("AZURE_BLOB_STORAGE_CONTAINER_NAME", "--azure-blob-storage-container-name")
                };
            }
        }
    );

var app = builder.Build();

try
{
    await app.RunAsync();
    return 0;
}
catch (MissingConfigurationValueException ex)
{
    appLogger.LogError("{Message}", ex.Message);

    return 1;
}
catch (Exception)
{
    return 1;
}
