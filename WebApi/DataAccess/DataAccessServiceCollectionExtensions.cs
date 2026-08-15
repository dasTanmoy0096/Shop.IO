namespace DataAccess;

using System;

using DataAccess.Configuration;

using Microsoft.Extensions.DependencyInjection;

public static class DataAccessServiceCollectionExtensions {
    public static IServiceCollection AddDataAccess(this IServiceCollection services) {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<MariaDbConnectionConfigurationValidator>();
        services.AddHostedService<MariaDbConfigurationValidationHostedService>();

        return services;
    }
}
