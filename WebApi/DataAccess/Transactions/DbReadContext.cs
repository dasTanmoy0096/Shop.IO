namespace DataAccess.Transactions;

using System;
using System.Data.Common;
using System.Threading.Tasks;

using DataAccess.Internals;

internal sealed class DbReadContext {
    private readonly DbConnection connection;

    internal DbReadContext(DbConnection connection) {
        ArgumentNullException.ThrowIfNull(connection);

        this.connection = connection;
    }

    internal Task<DbCommand> CreateTextCommandAsync(string commandText) {
        return DbCommandFactory.CreateTextCommandAsync(
            connection,
            null,
            commandText
        );
    }
}
