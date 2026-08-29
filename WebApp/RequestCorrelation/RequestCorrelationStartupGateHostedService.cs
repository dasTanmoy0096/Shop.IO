namespace WebApp.RequestCorrelation;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

internal sealed class RequestCorrelationStartupGateHostedService : IHostedService {
    private readonly WebAppRequestCorrelationConfiguration requestCorrelationConfiguration;

    public RequestCorrelationStartupGateHostedService(WebAppRequestCorrelationConfiguration requestCorrelationConfiguration) {
        ArgumentNullException.ThrowIfNull(requestCorrelationConfiguration);

        this.requestCorrelationConfiguration = requestCorrelationConfiguration;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        _ = requestCorrelationConfiguration.ResponseHeaderName;

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
