using Microsoft.Extensions.DependencyInjection;
using DatabaseBackupTool.ConsoleApp.Services;

namespace DatabaseBackupTool.ConsoleApp.Extensions;

/// <summary>
/// Extension methods for adding <see cref="MainService"/> to the service collection.
/// </summary>
internal static class MainServiceStartupExtensions
{
    /// <summary>
    /// Adds the <see cref="MainService"/> to the service collection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <param name="options">A delegate that is used to configure the <see cref="MainServiceOptions"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddMainService(this IServiceCollection services, Action<MainServiceOptions> options)
    {
        services.Configure(options);

        services.AddHostedService<MainService>();

        return services;
    }
}