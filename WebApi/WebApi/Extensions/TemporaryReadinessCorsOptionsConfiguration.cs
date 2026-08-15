namespace WebApi.Extensions;

using System;
using System.Collections.Generic;

using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

// TEMPORARY: Remove with the P3.07 readiness demonstration when P7 owns CORS configuration.
internal sealed class TemporaryReadinessCorsOptionsConfiguration : IConfigureOptions<CorsOptions> {
    internal const string PolicyName = "temporary-database-readiness";

    private readonly IConfiguration configuration;

    public TemporaryReadinessCorsOptionsConfiguration(IConfiguration configuration) {
        ArgumentNullException.ThrowIfNull(configuration);

        this.configuration = configuration;
    }

    public void Configure(CorsOptions options) {
        ArgumentNullException.ThrowIfNull(options);

        List<string> allowedOrigins = [];

        foreach (IConfigurationSection originSection in configuration
            .GetRequiredSection("Cors:AllowedOrigins")
            .GetChildren()) {
            if (string.IsNullOrWhiteSpace(originSection.Value)) {
                throw new InvalidOperationException("Cors:AllowedOrigins contains an empty origin.");
            }

            allowedOrigins.Add(originSection.Value);
        }

        if (allowedOrigins.Count == 0) {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin for the temporary readiness endpoint.");
        }

        options.AddPolicy(
            PolicyName,
            policyBuilder => policyBuilder
                .WithOrigins([ .. allowedOrigins ])
                .WithMethods(HttpMethods.Get)
        );
    }
}
