namespace DataAccess.Internals;

using System;
using System.Data.Common;

internal sealed class DataAccessDatabaseException : Exception {
    internal DataAccessDatabaseException(
        string operationIdentifier,
        DbException innerException
    ) : base(
        $"The data-access operation '{operationIdentifier}' failed.",
        innerException
    ) {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationIdentifier);
        ArgumentNullException.ThrowIfNull(innerException);
    }
}
