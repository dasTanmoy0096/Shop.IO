namespace WebApi.Extensions;

using System;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using WebApi.Security;

internal static class RequestSecurityServiceCollectionExtensions {
    internal static IServiceCollection AddShopIoRequestSecurity(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        services.AddCors();
        services.AddAntiforgery();
        services.AddRateLimiter(static _ => { });
        services.AddSingleton<WebApiRequestSecurityConfiguration>();
        services.AddSingleton<IConfigureOptions<CorsOptions>, WebApiCorsOptionsConfiguration>();
        services.AddSingleton<IConfigureOptions<AntiforgeryOptions>, WebApiAntiforgeryOptionsConfiguration>();
        services.AddSingleton<IConfigureOptions<MvcOptions>, WebApiMvcAntiforgeryOptionsConfiguration>();
        services.AddSingleton<IConfigureOptions<RateLimiterOptions>, WebApiRateLimiterOptionsConfiguration>();
        services.AddHostedService<RequestSecurityStartupGateHostedService>();

        return services;
    }
}
