namespace WebApi.Extensions;

using System;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

using WebApi.Authentication;

internal static class AuthenticationServiceCollectionExtensions {
    internal static IServiceCollection AddAccountAuthentication(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions();
        services.AddDataProtection();
        services.AddSingleton<WebApiAuthenticationConfiguration>();
        services.AddSingleton<IConfigureOptions<DataProtectionOptions>, WebApiDataProtectionOptionsConfiguration>();
        services.AddSingleton<IConfigureOptions<KeyManagementOptions>, WebApiDataProtectionKeyManagementOptionsConfiguration>();
        services.AddAuthentication(AccountAuthenticationDefaults.Scheme)
            .AddCookie(AccountAuthenticationDefaults.Scheme);
        services.AddSingleton<IConfigureOptions<CookieAuthenticationOptions>, AccountCookieAuthenticationOptionsConfiguration>();
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<AccountCookieTicketFactory>();
        services.AddScoped<AccountCookieAuthenticationEvents>();
        services.AddHostedService<AuthenticationStartupGateHostedService>();

        return services;
    }
}
