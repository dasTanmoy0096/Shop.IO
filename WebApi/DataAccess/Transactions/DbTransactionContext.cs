namespace DataAccess.Transactions;

using System;
using System.Data.Common;
using System.Threading.Tasks;

using DataAccess.Internals;

internal sealed class DbTransactionContext {
    private readonly DbConnection connection;
    private readonly DbTransaction transaction;

    internal DbTransactionContext(
        DbConnection connection,
        DbTransaction transaction
    ) {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentNullException.ThrowIfNull(transaction);

        if (!ReferenceEquals(
            transaction.Connection,
            connection
        )) {
            throw new ArgumentException(
                "The transaction must belong to the context connection.",
                nameof(transaction)
            );
        }

        this.connection = connection;
        this.transaction = transaction;
    }

    internal Task<DbCommand> CreateTextCommandAsync(string commandText) {
        return DbCommandFactory.CreateTextCommandAsync(
            connection,
            transaction,
            commandText
        );
    }
}
