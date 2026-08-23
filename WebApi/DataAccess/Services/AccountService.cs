namespace DataAccess.Services;

using System;
using System.Threading;
using System.Threading.Tasks;

using DataAccess.Configuration;
using DataAccess.Internals;
using DataAccess.Repositories;
using DataAccess.Transfers;

using Microsoft.AspNetCore.Identity;

internal sealed class AccountService : IAccountService {
    private readonly AccountPasswordHasher accountPasswordHasher;
    private readonly AccountPolicy accountPolicy;
    private readonly AccountRepository accountRepository;

    public AccountService(
        AccountPasswordHasher accountPasswordHasher,
        AccountPolicy accountPolicy,
        AccountRepository accountRepository
    ) {
        ArgumentNullException.ThrowIfNull(accountPasswordHasher);
        ArgumentNullException.ThrowIfNull(accountPolicy);
        ArgumentNullException.ThrowIfNull(accountRepository);

        this.accountPasswordHasher = accountPasswordHasher;
        this.accountPolicy = accountPolicy;
        this.accountRepository = accountRepository;
    }

    async Task<AccountRegistrationResult> IAccountService.RegisterAsync(
        AccountRegistrationRequest request,
        CancellationToken cancellationToken
    ) {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!accountPolicy.TryNormalizeUsernameForRegistration(
            request.Username,
            out AccountUsername? username
        )) {
            return new AccountRegistrationResult(
                AccountRegistrationStatus.InvalidUsername,
                null
            );
        }

        if (!accountPolicy.TryNormalizePasswordForRegistration(
            request.Password,
            username,
            out string normalizedPassword
        )) {
            return new AccountRegistrationResult(
                AccountRegistrationStatus.InvalidPassword,
                null
            );
        }

        Guid publicId = Guid.NewGuid();
        NewAccountRecord account = new(
            publicId,
            username,
            accountPasswordHasher.HashPassword(normalizedPassword),
            Guid.NewGuid()
        );
        bool wasCreated = await accountRepository.TryCreateAsync(
            account,
            cancellationToken
        );

        if (!wasCreated) {
            return new AccountRegistrationResult(
                AccountRegistrationStatus.UsernameUnavailable,
                null
            );
        }

        return new AccountRegistrationResult(
            AccountRegistrationStatus.Created,
            new AccountIdentity(
                publicId,
                username.Value
            )
        );
    }

    async Task<AccountCredentialVerificationResult> IAccountService.VerifyCredentialsAsync(
        AccountCredentialVerificationRequest request,
        CancellationToken cancellationToken
    ) {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (!AccountPolicy.TryNormalizeUsernameForVerification(
            request.Username,
            out AccountUsername? username
        )) {
            return new AccountCredentialVerificationResult(
                AccountCredentialVerificationStatus.InvalidCredentials,
                null
            );
        }

        if (!AccountPolicy.TryNormalizePasswordForVerification(
            request.Password,
            out string normalizedPassword
        )) {
            return new AccountCredentialVerificationResult(
                AccountCredentialVerificationStatus.InvalidCredentials,
                null
            );
        }

        AccountCredentialRecord? account = await accountRepository.FindByNormalizedUsernameAsync(
            username.NormalizedValue,
            cancellationToken
        );

        if (account is null) {
            accountPasswordHasher.ConsumeMissingAccountAttempt(normalizedPassword);

            return new AccountCredentialVerificationResult(
                AccountCredentialVerificationStatus.InvalidCredentials,
                null
            );
        }

        PasswordVerificationResult passwordVerificationResult = accountPasswordHasher.VerifyPassword(
            account.PasswordHash,
            normalizedPassword
        );

        if (passwordVerificationResult == PasswordVerificationResult.Failed || !account.IsActive) {
            return new AccountCredentialVerificationResult(
                AccountCredentialVerificationStatus.InvalidCredentials,
                null
            );
        }

        if (passwordVerificationResult == PasswordVerificationResult.SuccessRehashNeeded) {
            string replacementPasswordHash = accountPasswordHasher.HashPassword(normalizedPassword);

            _ = await accountRepository.TryUpgradePasswordHashAsync(
                account,
                replacementPasswordHash,
                cancellationToken
            );
        }

        return new AccountCredentialVerificationResult(
            AccountCredentialVerificationStatus.Authenticated,
            new AuthenticatedAccount(
                account.PublicId,
                account.Username,
                account.SecurityStamp,
                account.RoleCodes
            )
        );
    }

    async Task<AccountSessionInvalidationResult> IAccountService.InvalidateSessionsAsync(
        AccountSessionInvalidationRequest request,
        CancellationToken cancellationToken
    ) {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.AccountPublicId == Guid.Empty) {
            return new AccountSessionInvalidationResult(AccountSessionInvalidationStatus.AccountNotFound);
        }

        bool wasInvalidated = await accountRepository.TryInvalidateSessionsAsync(
            request.AccountPublicId,
            Guid.NewGuid(),
            cancellationToken
        );

        return new AccountSessionInvalidationResult(
            wasInvalidated
                ? AccountSessionInvalidationStatus.Invalidated
                : AccountSessionInvalidationStatus.AccountNotFound
        );
    }

    async Task<AccountSessionValidationResult> IAccountService.ValidateSessionAsync(
        AccountSessionValidationRequest request,
        CancellationToken cancellationToken
    ) {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (request.AccountPublicId == Guid.Empty || request.SecurityStamp == Guid.Empty) {
            return new AccountSessionValidationResult(AccountSessionValidationStatus.Invalid);
        }

        try {
            bool isValid = await accountRepository.IsSessionValidAsync(
                request.AccountPublicId,
                request.SecurityStamp,
                cancellationToken
            );

            return new AccountSessionValidationResult(
                isValid
                    ? AccountSessionValidationStatus.Valid
                    : AccountSessionValidationStatus.Invalid
            );
        } catch (DataAccessDatabaseException) {
            return new AccountSessionValidationResult(AccountSessionValidationStatus.Unavailable);
        } catch (System.Data.DataException) {
            return new AccountSessionValidationResult(AccountSessionValidationStatus.Unavailable);
        }
    }
}
