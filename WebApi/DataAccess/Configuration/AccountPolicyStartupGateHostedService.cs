namespace DataAccess.Configuration;

using System;
using System.Threading;
using System.Threading.Tasks;

using DataAccess.Internals;

using Microsoft.Extensions.Hosting;

internal sealed class AccountPolicyStartupGateHostedService : IHostedService {
    public AccountPolicyStartupGateHostedService(
        AccountPolicy accountPolicy,
        AccountPasswordHasher accountPasswordHasher
    ) {
        ArgumentNullException.ThrowIfNull(accountPolicy);
        ArgumentNullException.ThrowIfNull(accountPasswordHasher);
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
