namespace DataAccess.Configuration;

using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

internal sealed class MariaDbConfigurationValidationHostedService : IHostedService {
    private readonly MariaDbConnectionConfigurationValidator configurationValidator;

    internal MariaDbConfigurationValidationHostedService(
        MariaDbConnectionConfigurationValidator configurationValidator
    ) {
        ArgumentNullException.ThrowIfNull(configurationValidator);

        this.configurationValidator = configurationValidator;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();
        configurationValidator.Validate();

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
