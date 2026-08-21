namespace DataAccess.Internals;

using System;

using Microsoft.AspNetCore.Identity;

internal sealed class AccountPasswordHasher {
    private readonly AccountPasswordSubject passwordSubject = new();
    private readonly IPasswordHasher<AccountPasswordSubject> passwordHasher;
    private readonly string missingAccountPasswordHash;

    public AccountPasswordHasher(IPasswordHasher<AccountPasswordSubject> passwordHasher) {
        ArgumentNullException.ThrowIfNull(passwordHasher);

        this.passwordHasher = passwordHasher;
        missingAccountPasswordHash = passwordHasher.HashPassword(
            passwordSubject,
            Guid.NewGuid().ToString("N")
        );
    }

    internal string HashPassword(string password) {
        ArgumentNullException.ThrowIfNull(password);

        return passwordHasher.HashPassword(
            passwordSubject,
            password
        );
    }

    internal PasswordVerificationResult VerifyPassword(
        string passwordHash,
        string password
    ) {
        ArgumentNullException.ThrowIfNull(passwordHash);
        ArgumentNullException.ThrowIfNull(password);

        return passwordHasher.VerifyHashedPassword(
            passwordSubject,
            passwordHash,
            password
        );
    }

    internal void ConsumeMissingAccountAttempt(string password) {
        ArgumentNullException.ThrowIfNull(password);

        _ = passwordHasher.VerifyHashedPassword(
            passwordSubject,
            missingAccountPasswordHash,
            password
        );
    }
}
