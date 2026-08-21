namespace DataAccess;

using System;
using System.Data.Common;

using DataAccess.Configuration;
using DataAccess.Internals;
using DataAccess.Repositories;
using DataAccess.Services;
using DataAccess.Transactions;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class DataAccessServiceCollectionExtensions {
    public static IServiceCollection AddDataAccess(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        services.AddOptions();
        services.AddSingleton<MariaDbConnectionConfigurationValidator>();
        services.AddSingleton<MariaDbDataSourceFactory>();
        services.AddSingleton<AccountPolicy>();
        services.AddSingleton<IConfigureOptions<PasswordHasherOptions>, AccountPasswordHasherOptionsConfiguration>();
        services.AddSingleton<IPasswordHasher<AccountPasswordSubject>, PasswordHasher<AccountPasswordSubject>>();
        services.AddSingleton<AccountPasswordHasher>();
        services.AddSingleton<DbDataSource>(
            static serviceProvider => serviceProvider
                .GetRequiredService<MariaDbDataSourceFactory>()
                .Build()
        );
        services.AddSingleton<DbConnectionExecutor>();
        services.AddTransient<AccountRepository>();
        services.AddTransient<DatabaseReadinessRepository>();
        services.AddTransient<IAccountService, AccountService>();
        services.AddTransient<IDatabaseReadinessService, DatabaseReadinessService>();
        services.AddHostedService<AccountPolicyStartupGateHostedService>();
        services.AddHostedService<MariaDbDataSourceStartupGateHostedService>();

        return services;
    }
}
