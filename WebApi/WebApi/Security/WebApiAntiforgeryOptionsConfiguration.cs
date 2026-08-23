namespace WebApi.Security;

using System;

using Microsoft.AspNetCore.Antiforgery;
using Microsoft.Extensions.Options;

internal sealed class WebApiAntiforgeryOptionsConfiguration : IConfigureOptions<AntiforgeryOptions> {
    private readonly WebApiRequestSecurityConfiguration requestSecurityConfiguration;

    public WebApiAntiforgeryOptionsConfiguration(WebApiRequestSecurityConfiguration requestSecurityConfiguration) {
        ArgumentNullException.ThrowIfNull(requestSecurityConfiguration);

        this.requestSecurityConfiguration = requestSecurityConfiguration;
    }

    void IConfigureOptions<AntiforgeryOptions>.Configure(AntiforgeryOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        options.Cookie.Name = requestSecurityConfiguration.AntiforgeryCookieName;
        options.Cookie.Path = requestSecurityConfiguration.AntiforgeryCookiePath;
        options.Cookie.Domain = requestSecurityConfiguration.AntiforgeryCookieDomain;
        options.Cookie.HttpOnly = requestSecurityConfiguration.AntiforgeryCookieHttpOnly;
        options.Cookie.SameSite = requestSecurityConfiguration.AntiforgeryCookieSameSite;
        options.Cookie.SecurePolicy = requestSecurityConfiguration.AntiforgeryCookieSecurePolicy;
        options.Cookie.IsEssential = requestSecurityConfiguration.AntiforgeryCookieIsEssential;
        options.HeaderName = requestSecurityConfiguration.AntiforgeryHeaderName;
        options.FormFieldName = requestSecurityConfiguration.AntiforgeryFormFieldName;
        options.SuppressReadingTokenFromFormBody = requestSecurityConfiguration.AntiforgerySuppressReadingTokenFromFormBody;
        options.SuppressXFrameOptionsHeader = requestSecurityConfiguration.AntiforgerySuppressXFrameOptionsHeader;
    }
}
