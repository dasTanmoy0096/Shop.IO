namespace WebApi.Extensions;

using System;

using Microsoft.Extensions.DependencyInjection;

using WebApi.Controllers;

// TEMPORARY: Remove with the P3.07 readiness demonstration when P7 owns controller registration.
internal static class TemporaryReadinessServiceCollectionExtensions {
    internal static IServiceCollection AddTemporaryReadinessDemonstration(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        IMvcCoreBuilder mvcCoreBuilder = services.AddMvcCore();

        // Supplies the platform MVC antiforgery filter service without adding a Razor view engine.
        mvcCoreBuilder.AddViews();
        mvcCoreBuilder.AddControllersAsServices();
        services.AddScoped<TemporaryDatabaseReadinessController>();

        return services;
    }
}
