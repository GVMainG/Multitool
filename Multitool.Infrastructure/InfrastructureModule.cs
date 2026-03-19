using Microsoft.Extensions.DependencyInjection;
using Multitool.Core;

namespace Multitool.Infrastructure;

/// <summary>
/// Модуль регистрации зависимостей Infrastructure
/// </summary>
public static class InfrastructureModule
{
    /// <summary>
    /// Регистрация сервисов инфраструктуры в DI контейнере
    /// </summary>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddTransient<IAudioExtractor, AudioExtractor>();
        services.AddTransient<IHHService, HHService>();
        return services;
    }
}
