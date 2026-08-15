namespace DataAccess.Internals;

using System;
using System.Data.Common;

internal static class DbExceptionTranslator {
    internal static DataAccessDatabaseException Translate(
        DbException exception,
        string operationIdentifier
    ) {
        ArgumentNullException.ThrowIfNull(exception);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationIdentifier);

        return new DataAccessDatabaseException(
            operationIdentifier,
            exception
        );
    }
}
