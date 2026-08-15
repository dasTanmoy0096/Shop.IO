namespace DataAccess.Internals;

using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

internal static class DbDataReaderValueReader {
    internal static async Task<T> ReadRequiredAsync<T>(
        DbDataReader dataReader,
        int ordinal,
        string columnName,
        CancellationToken cancellationToken
    ) {
        ValidateReadArguments(
            dataReader,
            ordinal,
            columnName
        );

        cancellationToken.ThrowIfCancellationRequested();

        if (await dataReader.IsDBNullAsync(
            ordinal,
            cancellationToken
        )) {
            throw new DataException($"The required database column '{columnName}' is NULL.");
        }

        return await dataReader.GetFieldValueAsync<T>(
            ordinal,
            cancellationToken
        );
    }

    internal static async Task<T?> ReadOptionalAsync<T>(
        DbDataReader dataReader,
        int ordinal,
        string columnName,
        CancellationToken cancellationToken
    ) {
        ValidateReadArguments(
            dataReader,
            ordinal,
            columnName
        );

        cancellationToken.ThrowIfCancellationRequested();

        if (await dataReader.IsDBNullAsync(
            ordinal,
            cancellationToken
        )) {
            return default;
        }

        return await dataReader.GetFieldValueAsync<T>(
            ordinal,
            cancellationToken
        );
    }

    private static void ValidateReadArguments(
        DbDataReader dataReader,
        int ordinal,
        string columnName
    ) {
        ArgumentNullException.ThrowIfNull(dataReader);
        ArgumentOutOfRangeException.ThrowIfNegative(ordinal);
        ArgumentException.ThrowIfNullOrWhiteSpace(columnName);
    }
}
