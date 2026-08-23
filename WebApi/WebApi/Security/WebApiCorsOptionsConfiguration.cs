namespace WebApi.Security;

using System;

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.Options;

internal sealed class WebApiCorsOptionsConfiguration : IConfigureOptions<CorsOptions> {
    private readonly WebApiRequestSecurityConfiguration requestSecurityConfiguration;

    public WebApiCorsOptionsConfiguration(WebApiRequestSecurityConfiguration requestSecurityConfiguration) {
        ArgumentNullException.ThrowIfNull(requestSecurityConfiguration);

        this.requestSecurityConfiguration = requestSecurityConfiguration;
    }

    void IConfigureOptions<CorsOptions>.Configure(CorsOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        options.AddPolicy(
            CorsPolicyNames.Browser,
            policyBuilder => {
                policyBuilder
                    .WithOrigins([ .. requestSecurityConfiguration.CorsAllowedOrigins ])
                    .WithMethods([ .. requestSecurityConfiguration.CorsAllowedMethods ])
                    .WithHeaders([ .. requestSecurityConfiguration.CorsAllowedHeaders ])
                    .WithExposedHeaders([ .. requestSecurityConfiguration.CorsExposedHeaders ])
                    .SetPreflightMaxAge(requestSecurityConfiguration.CorsPreflightMaxAge);

                if (requestSecurityConfiguration.CorsAllowCredentials) {
                    policyBuilder.AllowCredentials();
                }
            }
        );
    }
}
