namespace DataAccess.Repositories;

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using DataAccess.Internals;
using DataAccess.Transactions;

internal sealed class DatabaseReadinessRepository {
    private const string OperationIdentifier = "database-readiness";
    private const string ReadinessCommandText = "SELECT 1";

    private readonly DbConnectionExecutor connectionExecutor;

    public DatabaseReadinessRepository(DbConnectionExecutor connectionExecutor) {
        ArgumentNullException.ThrowIfNull(connectionExecutor);

        this.connectionExecutor = connectionExecutor;
    }

    internal Task<bool> CheckReadinessAsync(CancellationToken cancellationToken) {
        return connectionExecutor.ExecuteReadAsync(
            OperationIdentifier,
            ExecuteReadinessProbeAsync,
            cancellationToken
        );
    }

    private static async Task<bool> ExecuteReadinessProbeAsync(
        DbReadContext readContext,
        CancellationToken cancellationToken
    ) {
        await using DbCommand command = await readContext.CreateTextCommandAsync(ReadinessCommandText);
        await using DbDataReader dataReader = await DbCommandExecutor.ExecuteReaderAsync(
            command,
            CommandBehavior.SingleRow,
            OperationIdentifier,
            cancellationToken
        );

        if (!await dataReader.ReadAsync(cancellationToken)) {
            throw new DataException("The database readiness probe returned no row.");
        }

        return true;
    }
}
