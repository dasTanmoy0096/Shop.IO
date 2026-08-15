namespace DataAccess.Internals;

using System;
using System.Data;
using System.Data.Common;
using System.Threading.Tasks;

internal static class DbCommandFactory {
    internal static async Task<DbCommand> CreateTextCommandAsync(
        DbConnection connection,
        DbTransaction? transaction,
        string commandText
    ) {
        ArgumentNullException.ThrowIfNull(connection);
        ArgumentException.ThrowIfNullOrWhiteSpace(commandText);

        if (transaction is not null && !ReferenceEquals(
            transaction.Connection,
            connection
        )) {
            throw new ArgumentException(
                "The transaction must belong to the command connection.",
                nameof(transaction)
            );
        }

        DbCommand command = connection.CreateCommand();

        try {
            command.CommandType = CommandType.Text;
            command.CommandText = commandText;
            command.Transaction = transaction;

            return command;
        } catch {
            await command.DisposeAsync();
            throw;
        }
    }
}
