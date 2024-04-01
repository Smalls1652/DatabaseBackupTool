using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using DatabaseBackupTool.ConsoleApp.Hosting;

namespace DatabaseBackupTool.ConsoleApp.Extensions;

/// <summary>
/// Extension methods for adding the <see cref="SlimHostLifetime"/> to the <see cref="IServiceCollection"/>.
/// </summary>
internal static class SlimHostLifetimeExtensions
{
    /// <summary>
    /// Adds the <see cref="SlimHostLifetime"/> to the <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the service to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddSlimHostLifetime(this IServiceCollection services)
    {
        services.RemoveAll<IHostLifetime>();

        services.AddSingleton<IHostLifetime, SlimHostLifetime>();

        return services;
    }
}