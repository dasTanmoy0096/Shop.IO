namespace DataAccess.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

using DataAccess.Internals;
using DataAccess.Transactions;

internal sealed class AccountRepository {
    private const string CreateAccountOperationIdentifier = "account-create";
    private const string FindAccountByUsernameOperationIdentifier = "account-find-by-normalized-username";
    private const string UpgradePasswordHashOperationIdentifier = "account-upgrade-password-hash";
    private const string InvalidateSessionsOperationIdentifier = "account-invalidate-sessions";
    private const string ValidateSessionOperationIdentifier = "account-validate-session";

    private const string FindExistingNormalizedUsernameCommandText = """
        SELECT `account_id`
        FROM `account`
        WHERE `normalized_username` = @normalizedUsername
        FOR UPDATE
        """;

    private const string InsertAccountCommandText = """
        INSERT INTO `account` (
            `public_id`,
            `username`,
            `normalized_username`,
            `password_hash`,
            `security_stamp`,
            `is_active`
        )
        VALUES (
            @publicId,
            @username,
            @normalizedUsername,
            @passwordHash,
            @securityStamp,
            @isActive
        )
        """;

    private const string FindAccountByNormalizedUsernameCommandText = """
        SELECT
            `account`.`account_id`,
            `account`.`public_id`,
            `account`.`username`,
            `account`.`password_hash`,
            `account`.`security_stamp`,
            `account`.`is_active`,
            `accountRoleAssignment`.`role_code`
        FROM `account` AS `account`
        LEFT JOIN `account_role_assignment` AS `accountRoleAssignment`
            ON `accountRoleAssignment`.`account_id` = `account`.`account_id`
        WHERE `account`.`normalized_username` = @normalizedUsername
        ORDER BY `accountRoleAssignment`.`role_code`
        """;

    private const string UpgradePasswordHashCommandText = """
        UPDATE `account`
        SET
            `password_hash` = @replacementPasswordHash,
            `updated_utc` = CURRENT_TIMESTAMP(6)
        WHERE `account_id` = @accountId
            AND `password_hash` = @expectedPasswordHash
            AND `is_active` = @isActive
        """;

    private const string InvalidateSessionsCommandText = """
        UPDATE `account`
        SET
            `security_stamp` = @securityStamp,
            `updated_utc` = CURRENT_TIMESTAMP(6)
        WHERE `public_id` = @publicId
        """;

    private const string ValidateSessionCommandText = """
        SELECT 1
        FROM `account`
        WHERE `public_id` = @publicId
            AND `security_stamp` = @securityStamp
            AND `is_active` = @isActive
        LIMIT 1
        """;

    private readonly DbConnectionExecutor connectionExecutor;

    public AccountRepository(DbConnectionExecutor connectionExecutor) {
        ArgumentNullException.ThrowIfNull(connectionExecutor);

        this.connectionExecutor = connectionExecutor;
    }

    internal Task<bool> TryCreateAsync(
        NewAccountRecord account,
        CancellationToken cancellationToken
    ) {
        ArgumentNullException.ThrowIfNull(account);

        return connectionExecutor.ExecuteTransactionAsync(
            CreateAccountOperationIdentifier,
            (transactionContext, operationCancellationToken) => TryCreateAsync(
                transactionContext,
                account,
                operationCancellationToken
            ),
            cancellationToken
        );
    }

    internal Task<AccountCredentialRecord?> FindByNormalizedUsernameAsync(
        string normalizedUsername,
        CancellationToken cancellationToken
    ) {
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedUsername);

        return connectionExecutor.ExecuteReadAsync(
            FindAccountByUsernameOperationIdentifier,
            (readContext, operationCancellationToken) => FindByNormalizedUsernameAsync(
                readContext,
                normalizedUsername,
                operationCancellationToken
            ),
            cancellationToken
        );
    }

    internal Task<bool> TryUpgradePasswordHashAsync(
        AccountCredentialRecord account,
        string replacementPasswordHash,
        CancellationToken cancellationToken
    ) {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentException.ThrowIfNullOrWhiteSpace(replacementPasswordHash);

        return connectionExecutor.ExecuteTransactionAsync(
            UpgradePasswordHashOperationIdentifier,
            (transactionContext, operationCancellationToken) => TryUpgradePasswordHashAsync(
                transactionContext,
                account,
                replacementPasswordHash,
                operationCancellationToken
            ),
            cancellationToken
        );
    }

    internal Task<bool> TryInvalidateSessionsAsync(
        Guid publicId,
        Guid securityStamp,
        CancellationToken cancellationToken
    ) {
        if (publicId == Guid.Empty) {
            throw new ArgumentOutOfRangeException(
                nameof(publicId),
                "The public identifier must not be empty."
            );
        }

        if (securityStamp == Guid.Empty) {
            throw new ArgumentOutOfRangeException(
                nameof(securityStamp),
                "The security stamp must not be empty."
            );
        }

        return connectionExecutor.ExecuteTransactionAsync(
            InvalidateSessionsOperationIdentifier,
            (transactionContext, operationCancellationToken) => TryInvalidateSessionsAsync(
                transactionContext,
                publicId,
                securityStamp,
                operationCancellationToken
            ),
            cancellationToken
        );
    }

    internal Task<bool> IsSessionValidAsync(
        Guid publicId,
        Guid securityStamp,
        CancellationToken cancellationToken
    ) {
        if (publicId == Guid.Empty) {
            throw new ArgumentOutOfRangeException(
                nameof(publicId),
                "The public identifier must not be empty."
            );
        }

        if (securityStamp == Guid.Empty) {
            throw new ArgumentOutOfRangeException(
                nameof(securityStamp),
                "The security stamp must not be empty."
            );
        }

        return connectionExecutor.ExecuteReadAsync(
            ValidateSessionOperationIdentifier,
            (readContext, operationCancellationToken) => IsSessionValidAsync(
                readContext,
                publicId,
                securityStamp,
                operationCancellationToken
            ),
            cancellationToken
        );
    }

    private static async Task<bool> TryCreateAsync(
        DbTransactionContext transactionContext,
        NewAccountRecord account,
        CancellationToken cancellationToken
    ) {
        await using (DbCommand existingAccountCommand = await transactionContext.CreateTextCommandAsync(FindExistingNormalizedUsernameCommandText)) {
            DbParameterFactory.AddInputParameter(
                existingAccountCommand,
                "@normalizedUsername",
                DbType.String,
                account.Username.NormalizedValue
            );

            await using DbDataReader existingAccountReader = await DbCommandExecutor.ExecuteReaderAsync(
                existingAccountCommand,
                CommandBehavior.SingleRow,
                CreateAccountOperationIdentifier,
                cancellationToken
            );

            if (await existingAccountReader.ReadAsync(cancellationToken)) {
                return false;
            }
        }

        await using DbCommand insertAccountCommand = await transactionContext.CreateTextCommandAsync(InsertAccountCommandText);
        DbParameterFactory.AddInputParameter(
            insertAccountCommand,
            "@publicId",
            DbType.String,
            account.PublicId.ToString("D")
        );
        DbParameterFactory.AddInputParameter(
            insertAccountCommand,
            "@username",
            DbType.String,
            account.Username.Value
        );
        DbParameterFactory.AddInputParameter(
            insertAccountCommand,
            "@normalizedUsername",
            DbType.String,
            account.Username.NormalizedValue
        );
        DbParameterFactory.AddInputParameter(
            insertAccountCommand,
            "@passwordHash",
            DbType.String,
            account.PasswordHash
        );
        DbParameterFactory.AddInputParameter(
            insertAccountCommand,
            "@securityStamp",
            DbType.String,
            account.SecurityStamp.ToString("D")
        );
        DbParameterFactory.AddInputParameter(
            insertAccountCommand,
            "@isActive",
            DbType.Boolean,
            true
        );

        int affectedRowCount = await DbCommandExecutor.ExecuteNonQueryAsync(
            insertAccountCommand,
            CreateAccountOperationIdentifier,
            cancellationToken
        );

        if (affectedRowCount != 1) {
            throw new DataException("The account-create operation did not insert exactly one row.");
        }

        return true;
    }

    private static async Task<AccountCredentialRecord?> FindByNormalizedUsernameAsync(
        DbReadContext readContext,
        string normalizedUsername,
        CancellationToken cancellationToken
    ) {
        await using DbCommand command = await readContext.CreateTextCommandAsync(FindAccountByNormalizedUsernameCommandText);
        DbParameterFactory.AddInputParameter(
            command,
            "@normalizedUsername",
            DbType.String,
            normalizedUsername
        );

        await using DbDataReader dataReader = await DbCommandExecutor.ExecuteReaderAsync(
            command,
            CommandBehavior.Default,
            FindAccountByUsernameOperationIdentifier,
            cancellationToken
        );

        if (!await dataReader.ReadAsync(cancellationToken)) {
            return null;
        }

        long accountId = await DbDataReaderValueReader.ReadRequiredAsync<long>(
            dataReader,
            0,
            "account_id",
            cancellationToken
        );
        string publicId = await DbDataReaderValueReader.ReadRequiredAsync<string>(
            dataReader,
            1,
            "public_id",
            cancellationToken
        );
        string username = await DbDataReaderValueReader.ReadRequiredAsync<string>(
            dataReader,
            2,
            "username",
            cancellationToken
        );
        string passwordHash = await DbDataReaderValueReader.ReadRequiredAsync<string>(
            dataReader,
            3,
            "password_hash",
            cancellationToken
        );
        string securityStamp = await DbDataReaderValueReader.ReadRequiredAsync<string>(
            dataReader,
            4,
            "security_stamp",
            cancellationToken
        );
        bool isActive = await DbDataReaderValueReader.ReadRequiredAsync<bool>(
            dataReader,
            5,
            "is_active",
            cancellationToken
        );
        List<string> roleCodes = [];
        string? firstRoleCode = await DbDataReaderValueReader.ReadOptionalAsync<string>(
            dataReader,
            6,
            "role_code",
            cancellationToken
        );

        if (firstRoleCode is not null) {
            roleCodes.Add(firstRoleCode);
        }

        while (await dataReader.ReadAsync(cancellationToken)) {
            roleCodes.Add(await DbDataReaderValueReader.ReadRequiredAsync<string>(
                dataReader,
                6,
                "role_code",
                cancellationToken
            ));
        }

        return new AccountCredentialRecord(
            accountId,
            publicId,
            username,
            passwordHash,
            securityStamp,
            isActive,
            roleCodes
        );
    }

    private static async Task<bool> IsSessionValidAsync(
        DbReadContext readContext,
        Guid publicId,
        Guid securityStamp,
        CancellationToken cancellationToken
    ) {
        await using DbCommand command = await readContext.CreateTextCommandAsync(ValidateSessionCommandText);
        DbParameterFactory.AddInputParameter(
            command,
            "@publicId",
            DbType.String,
            publicId.ToString("D")
        );
        DbParameterFactory.AddInputParameter(
            command,
            "@securityStamp",
            DbType.String,
            securityStamp.ToString("D")
        );
        DbParameterFactory.AddInputParameter(
            command,
            "@isActive",
            DbType.Boolean,
            true
        );

        await using DbDataReader dataReader = await DbCommandExecutor.ExecuteReaderAsync(
            command,
            CommandBehavior.SingleRow,
            ValidateSessionOperationIdentifier,
            cancellationToken
        );

        return await dataReader.ReadAsync(cancellationToken);
    }

    private static async Task<bool> TryUpgradePasswordHashAsync(
        DbTransactionContext transactionContext,
        AccountCredentialRecord account,
        string replacementPasswordHash,
        CancellationToken cancellationToken
    ) {
        await using DbCommand command = await transactionContext.CreateTextCommandAsync(UpgradePasswordHashCommandText);
        DbParameterFactory.AddInputParameter(
            command,
            "@replacementPasswordHash",
            DbType.String,
            replacementPasswordHash
        );
        DbParameterFactory.AddInputParameter(
            command,
            "@accountId",
            DbType.Int64,
            account.AccountId
        );
        DbParameterFactory.AddInputParameter(
            command,
            "@expectedPasswordHash",
            DbType.String,
            account.PasswordHash
        );
        DbParameterFactory.AddInputParameter(
            command,
            "@isActive",
            DbType.Boolean,
            true
        );

        int affectedRowCount = await DbCommandExecutor.ExecuteNonQueryAsync(
            command,
            UpgradePasswordHashOperationIdentifier,
            cancellationToken
        );

        return affectedRowCount switch {
            0 => false,
            1 => true,
            _ => throw new DataException("The account-upgrade-password-hash operation affected more than one row."),
        };
    }

    private static async Task<bool> TryInvalidateSessionsAsync(
        DbTransactionContext transactionContext,
        Guid publicId,
        Guid securityStamp,
        CancellationToken cancellationToken
    ) {
        await using DbCommand command = await transactionContext.CreateTextCommandAsync(InvalidateSessionsCommandText);
        DbParameterFactory.AddInputParameter(
            command,
            "@publicId",
            DbType.String,
            publicId.ToString("D")
        );
        DbParameterFactory.AddInputParameter(
            command,
            "@securityStamp",
            DbType.String,
            securityStamp.ToString("D")
        );

        int affectedRowCount = await DbCommandExecutor.ExecuteNonQueryAsync(
            command,
            InvalidateSessionsOperationIdentifier,
            cancellationToken
        );

        return affectedRowCount switch {
            0 => false,
            1 => true,
            _ => throw new DataException("The account-invalidate-sessions operation affected more than one row."),
        };
    }
}
