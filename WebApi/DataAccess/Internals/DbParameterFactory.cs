namespace DataAccess.Internals;

using System;
using System.Data;
using System.Data.Common;

internal static class DbParameterFactory {
    internal static DbParameter AddInputParameter(
        DbCommand command,
        string parameterName,
        DbType dbType,
        object? value
    ) {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(parameterName);

        DbParameter parameter = command.CreateParameter();
        parameter.ParameterName = parameterName;
        parameter.Direction = ParameterDirection.Input;
        parameter.DbType = dbType;
        parameter.Value = value ?? DBNull.Value;

        command.Parameters.Add(parameter);

        return parameter;
    }
}
