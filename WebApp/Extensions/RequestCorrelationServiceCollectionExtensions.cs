namespace WebApp.Extensions;

using System;

using Microsoft.Extensions.DependencyInjection;

using WebApp.RequestCorrelation;

internal static class RequestCorrelationServiceCollectionExtensions {
    internal static IServiceCollection AddShopIoRequestCorrelation(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<WebAppRequestCorrelationConfiguration>();
        services.AddTransient<RequestCorrelationMiddleware>();
        services.AddHostedService<RequestCorrelationStartupGateHostedService>();

        return services;
    }
}
