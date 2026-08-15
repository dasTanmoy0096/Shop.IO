namespace DataAccess;

using System;
using System.Data.Common;

using DataAccess.Configuration;
using DataAccess.Internals;
using DataAccess.Repositories;
using DataAccess.Services;
using DataAccess.Transactions;

using Microsoft.Extensions.DependencyInjection;

public static class DataAccessServiceCollectionExtensions {
    public static IServiceCollection AddDataAccess(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<MariaDbConnectionConfigurationValidator>();
        services.AddSingleton<MariaDbDataSourceFactory>();
        services.AddSingleton<DbDataSource>(
            static serviceProvider => serviceProvider
                .GetRequiredService<MariaDbDataSourceFactory>()
                .Build()
        );
        services.AddSingleton<DbConnectionExecutor>();
        services.AddTransient<DatabaseReadinessRepository>();
        services.AddTransient<IDatabaseReadinessService, DatabaseReadinessService>();
        services.AddHostedService<MariaDbDataSourceStartupGateHostedService>();

        return services;
    }
}
