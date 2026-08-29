namespace WebApi.Extensions;

using System;

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

using WebApi.Middleware;

internal static class ErrorHandlingServiceCollectionExtensions {
    internal static IServiceCollection AddShopIoErrorHandling(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails();
        services.AddSingleton<WebApiErrorHandlingConfiguration>();
        services.AddSingleton<WebApiProblemDetailsResponseWriter>();
        services.AddTransient<RequestCorrelationMiddleware>();
        services.AddTransient<StatusCodeProblemDetailsMiddleware>();
        services.AddExceptionHandler<WebApiExceptionHandler>();
        services.Configure<ExceptionHandlerOptions>(
            static options => {
                options.SuppressDiagnosticsCallback = static _ => true;
            }
        );
        services.AddHostedService<ErrorHandlingStartupGateHostedService>();

        return services;
    }
}
