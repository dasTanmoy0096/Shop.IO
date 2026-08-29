namespace WebApi.Middleware;

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

using WebApi.Security;

internal sealed class ErrorHandlingStartupGateHostedService : IHostedService {
    private readonly WebApiErrorHandlingConfiguration errorHandlingConfiguration;
    private readonly WebApiRequestSecurityConfiguration requestSecurityConfiguration;

    public ErrorHandlingStartupGateHostedService(
        WebApiErrorHandlingConfiguration errorHandlingConfiguration,
        WebApiRequestSecurityConfiguration requestSecurityConfiguration
    ) {
        ArgumentNullException.ThrowIfNull(errorHandlingConfiguration);
        ArgumentNullException.ThrowIfNull(requestSecurityConfiguration);

        this.errorHandlingConfiguration = errorHandlingConfiguration;
        this.requestSecurityConfiguration = requestSecurityConfiguration;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        if (!ContainsHeader(
            requestSecurityConfiguration.CorsExposedHeaders,
            errorHandlingConfiguration.RequestIdResponseHeaderName
        )) {
            throw new InvalidOperationException("Cors:ExposedHeaders must contain the configured ErrorHandling:RequestIdResponseHeaderName.");
        }
        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }

    private static bool ContainsHeader(
        IReadOnlyList<string> headers,
        string requiredHeader
    ) {
        foreach (string header in headers) {
            if (string.Equals(
                header,
                requiredHeader,
                StringComparison.OrdinalIgnoreCase
            )) {
                return true;
            }
        }

        return false;
    }
}
