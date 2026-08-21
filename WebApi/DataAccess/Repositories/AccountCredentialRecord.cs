namespace DataAccess.Repositories;

using System;

internal sealed class AccountCredentialRecord {
    internal long AccountId { get; }
    internal string PublicId { get; }
    internal string Username { get; }
    internal string PasswordHash { get; }
    internal string SecurityStamp { get; }
    internal bool IsActive { get; }

    internal AccountCredentialRecord(
        long accountId,
        string publicId,
        string username,
        string passwordHash,
        string securityStamp,
        bool isActive
    ) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(accountId);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicId);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(securityStamp);

        AccountId = accountId;
        PublicId = publicId;
        Username = username;
        PasswordHash = passwordHash;
        SecurityStamp = securityStamp;
        IsActive = isActive;
    }

    public override string ToString() {
        return nameof(AccountCredentialRecord);
    }
}
