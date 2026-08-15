namespace WebApi.Extensions;

using System;

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using WebApi.Controllers;

// TEMPORARY: Remove with the P3.07 readiness demonstration when P7 owns controller/CORS registration.
internal static class TemporaryReadinessServiceCollectionExtensions {
    internal static IServiceCollection AddTemporaryReadinessDemonstration(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        IMvcCoreBuilder mvcCoreBuilder = services.AddMvcCore();

        mvcCoreBuilder.AddControllersAsServices();
        services.AddScoped<TemporaryDatabaseReadinessController>();
        services.AddCors();
        services.AddSingleton<IConfigureOptions<CorsOptions>, TemporaryReadinessCorsOptionsConfiguration>();

        return services;
    }
}
