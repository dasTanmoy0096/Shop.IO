namespace DataAccess.Transactions;

using System;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using DataAccess.Internals;

internal sealed class DbConnectionExecutor {
    private readonly DbDataSource dataSource;

    internal DbConnectionExecutor(DbDataSource dataSource) {
        ArgumentNullException.ThrowIfNull(dataSource);

        this.dataSource = dataSource;
    }

    internal async Task<TResult> ExecuteReadAsync<TResult>(
        string operationIdentifier,
        Func<DbReadContext, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken
    ) {
        ValidateOperationArguments(
            operationIdentifier,
            operation
        );
        cancellationToken.ThrowIfCancellationRequested();

        try {
            await using DbConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);

            return await operation(
                new DbReadContext(connection),
                cancellationToken
            );
        } catch (DbException exception) {
            throw DbExceptionTranslator.Translate(
                exception,
                operationIdentifier
            );
        }
    }

    internal async Task<TResult> ExecuteTransactionAsync<TResult>(
        string operationIdentifier,
        Func<DbTransactionContext, CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken
    ) {
        ValidateOperationArguments(
            operationIdentifier,
            operation
        );
        cancellationToken.ThrowIfCancellationRequested();

        try {
            await using DbConnection connection = await dataSource.OpenConnectionAsync(cancellationToken);
            await using DbTransaction transaction = await connection.BeginTransactionAsync(cancellationToken);

            try {
                TResult result = await operation(
                    new DbTransactionContext(
                        connection,
                        transaction
                    ),
                    cancellationToken
                );

                await transaction.CommitAsync(cancellationToken);

                return result;
            } catch (Exception operationException) {
                await RollbackAfterFailureAsync(
                    transaction,
                    operationIdentifier,
                    operationException
                );
                throw;
            }
        } catch (DbException exception) {
            throw DbExceptionTranslator.Translate(
                exception,
                operationIdentifier
            );
        }
    }

    private static async Task RollbackAfterFailureAsync(
        DbTransaction transaction,
        string operationIdentifier,
        Exception operationException
    ) {
        try {
            await transaction.RollbackAsync(CancellationToken.None);
        } catch (Exception rollbackException) {
            throw new DataAccessTransactionRollbackException(
                operationIdentifier,
                operationException,
                rollbackException
            );
        }
    }

    private static void ValidateOperationArguments<TContext, TResult>(
        string operationIdentifier,
        Func<TContext, CancellationToken, Task<TResult>> operation
    ) {
        ArgumentException.ThrowIfNullOrWhiteSpace(operationIdentifier);
        ArgumentNullException.ThrowIfNull(operation);
    }
}
