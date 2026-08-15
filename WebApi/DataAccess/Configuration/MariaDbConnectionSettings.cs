namespace DataAccess.Configuration;

using System;

internal sealed class MariaDbConnectionSettings {
    internal string ConnectionString { get; }
    internal string DataSourceName { get; }

    internal MariaDbConnectionSettings(string connectionString, string dataSourceName) {
        ArgumentException.ThrowIfNullOrWhiteSpace(connectionString);
        ArgumentException.ThrowIfNullOrWhiteSpace(dataSourceName);

        ConnectionString = connectionString;
        DataSourceName = dataSourceName;
    }
}
