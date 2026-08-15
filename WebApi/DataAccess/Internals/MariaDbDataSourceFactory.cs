namespace DataAccess.Internals;

using System;
using System.Data.Common;

using DataAccess.Configuration;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using MySqlConnector;

internal sealed class MariaDbDataSourceFactory {
    private readonly IConfiguration configuration;
    private readonly ILoggerFactory loggerFactory;
    private readonly MariaDbConnectionConfigurationValidator configurationValidator;

    public MariaDbDataSourceFactory(
        IConfiguration configuration,
        ILoggerFactory loggerFactory,
        MariaDbConnectionConfigurationValidator configurationValidator
    ) {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(loggerFactory);
        ArgumentNullException.ThrowIfNull(configurationValidator);

        this.configuration = configuration;
        this.loggerFactory = loggerFactory;
        this.configurationValidator = configurationValidator;
    }

    internal DbDataSource Build() {
        MariaDbConnectionSettings connectionSettings = configurationValidator.Validate(configuration);
        MySqlDataSourceBuilder dataSourceBuilder = new(connectionSettings.ConnectionString);

        dataSourceBuilder.UseName(connectionSettings.DataSourceName);
        dataSourceBuilder.UseLoggerFactory(loggerFactory);

        return dataSourceBuilder.Build();
    }
}
