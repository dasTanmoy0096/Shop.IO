namespace DataAccess.Configuration;

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;

internal sealed class MariaDbDataSourceStartupGateHostedService : IHostedService {
    public MariaDbDataSourceStartupGateHostedService(DbDataSource dataSource) {
        ArgumentNullException.ThrowIfNull(dataSource);
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken) {
        cancellationToken.ThrowIfCancellationRequested();

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken) {
        return Task.CompletedTask;
    }
}
