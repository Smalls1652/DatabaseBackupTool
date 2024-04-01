namespace DatabaseBackupTool.ConsoleApp.Models;

/// <summary>
/// The type of authentication to use when connecting to Azure.
/// </summary>
public enum AzureAuthType
{
    ConnectionString = 0,
    SystemManagedIdentity = 1,
    UserManagedIdentity = 2,
    ServicePrincipal = 3,
    AzureCli = 4
}