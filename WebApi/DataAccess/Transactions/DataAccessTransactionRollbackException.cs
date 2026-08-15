namespace DataAccess.Transactions;

using System;

internal sealed class DataAccessTransactionRollbackException : Exception {
    internal DataAccessTransactionRollbackException(
        string operationIdentifier,
        Exception operationException,
        Exception rollbackException
    ) : base(
        CreateMessage(operationIdentifier),
        CreateInnerException(
            operationException,
            rollbackException
        )
    ) { }

    private static string CreateMessage(string operationIdentifier) {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationIdentifier);

        return $"The local transaction rollback after data-access operation '{operationIdentifier}' failed.";
    }

    private static AggregateException CreateInnerException(
        Exception operationException,
        Exception rollbackException
    ) {
        ArgumentNullException.ThrowIfNull(operationException);
        ArgumentNullException.ThrowIfNull(rollbackException);

        return new AggregateException(operationException, rollbackException);
    }
}
