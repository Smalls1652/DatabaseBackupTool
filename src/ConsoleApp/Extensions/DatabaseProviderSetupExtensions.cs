using DatabaseBackupTool.ConsoleApp.DatabaseProviders;
using Microsoft.Extensions.DependencyInjection;

namespace DatabaseBackupTool.ConsoleApp.Extensions;

/// <summary>
/// Extension methods for setting up database providers.
/// </summary>
internal static class DatabaseProviderSetupExtensions
{
    /// <summary>
    /// Adds the <see cref="PostgresDatabaseProvider"/> as the database provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The options for the Postgres database provider.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddPostgresDatabaseProvider(this IServiceCollection services, Action<PostgresDatabaseProviderOptions> options)
    {
        services.Configure(options);
        
        services.AddTransient<IDatabaseProvider, PostgresDatabaseProvider>();

        return services;
    }
}