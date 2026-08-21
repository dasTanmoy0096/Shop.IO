namespace WebApi.Authentication;

using System;

using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;

internal sealed class AccountCookieAuthenticationOptionsConfiguration : IConfigureNamedOptions<CookieAuthenticationOptions> {
    private readonly WebApiAuthenticationConfiguration authenticationConfiguration;

    public AccountCookieAuthenticationOptionsConfiguration(
        WebApiAuthenticationConfiguration authenticationConfiguration
    ) {
        ArgumentNullException.ThrowIfNull(authenticationConfiguration);

        this.authenticationConfiguration = authenticationConfiguration;
    }

    public void Configure(CookieAuthenticationOptions options) {
        Configure(
            AccountAuthenticationDefaults.Scheme,
            options
        );
    }

    public void Configure(
        string? name,
        CookieAuthenticationOptions options
    ) {
        ArgumentNullException.ThrowIfNull(options);

        if (!string.Equals(
            name,
            AccountAuthenticationDefaults.Scheme,
            StringComparison.Ordinal
        )) {
            return;
        }

        options.Cookie.Name = authenticationConfiguration.CookieName;
        options.Cookie.Path = authenticationConfiguration.CookiePath;
        options.Cookie.Domain = null;
        options.Cookie.HttpOnly = authenticationConfiguration.CookieHttpOnly;
        options.Cookie.SameSite = authenticationConfiguration.CookieSameSite;
        options.Cookie.SecurePolicy = authenticationConfiguration.CookieSecurePolicy;
        options.Cookie.IsEssential = authenticationConfiguration.CookieIsEssential;
        options.ExpireTimeSpan = authenticationConfiguration.CookieLifetime;
        options.SlidingExpiration = authenticationConfiguration.SlidingExpiration;
        options.EventsType = typeof(AccountCookieAuthenticationEvents);
    }
}
