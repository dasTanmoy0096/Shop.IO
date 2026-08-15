namespace DataAccess.Internals;

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

internal static class DbCommandExecutor {
    internal static async Task<int> ExecuteNonQueryAsync(
        DbCommand command,
        string operationIdentifier,
        CancellationToken cancellationToken
    ) {
        ValidateExecutionArguments(
            command,
            operationIdentifier
        );
        cancellationToken.ThrowIfCancellationRequested();

        try {
            return await command.ExecuteNonQueryAsync(cancellationToken);
        } catch (DbException exception) {
            throw DbExceptionTranslator.Translate(
                exception,
                operationIdentifier
            );
        }
    }

    internal static async Task<DbDataReader> ExecuteReaderAsync(
        DbCommand command,
        CommandBehavior commandBehavior,
        string operationIdentifier,
        CancellationToken cancellationToken
    ) {
        ValidateExecutionArguments(
            command,
            operationIdentifier
        );
        cancellationToken.ThrowIfCancellationRequested();

        try {
            return await command.ExecuteReaderAsync(
                commandBehavior,
                cancellationToken
            );
        } catch (DbException exception) {
            throw DbExceptionTranslator.Translate(
                exception,
                operationIdentifier
            );
        }
    }

    internal static async Task<DbVersionedMutationOutcome> ExecuteVersionedSingleRowMutationAsync(
        DbCommand command,
        string operationIdentifier,
        CancellationToken cancellationToken
    ) {
        int affectedRowCount = await ExecuteNonQueryAsync(
            command,
            operationIdentifier,
            cancellationToken
        );

        return affectedRowCount switch {
            1 => DbVersionedMutationOutcome.Applied,
            0 => DbVersionedMutationOutcome.NotFoundOrVersionMismatch,
            _ => throw new DataException($"The versioned single-row data-access operation '{operationIdentifier}' affected {affectedRowCount} rows."),
        };
    }

    private static void ValidateExecutionArguments(
        DbCommand command,
        string operationIdentifier
    ) {
        ArgumentNullException.ThrowIfNull(command);
        ArgumentException.ThrowIfNullOrWhiteSpace(operationIdentifier);
    }
}
