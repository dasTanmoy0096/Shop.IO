namespace DataAccess.Repositories;

using System;
using System.Collections.Generic;

internal sealed class AccountCredentialRecord {
    internal long AccountId { get; }
    internal string PublicId { get; }
    internal string Username { get; }
    internal string PasswordHash { get; }
    internal string SecurityStamp { get; }
    internal bool IsActive { get; }
    internal IReadOnlyList<string> RoleCodes { get; }

    internal AccountCredentialRecord(
        long accountId,
        string publicId,
        string username,
        string passwordHash,
        string securityStamp,
        bool isActive,
        IEnumerable<string> roleCodes
    ) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(accountId);
        ArgumentException.ThrowIfNullOrWhiteSpace(publicId);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentException.ThrowIfNullOrWhiteSpace(securityStamp);
        ArgumentNullException.ThrowIfNull(roleCodes);

        List<string> copiedRoleCodes = [];

        foreach (string roleCode in roleCodes) {
            ArgumentException.ThrowIfNullOrWhiteSpace(roleCode);
            copiedRoleCodes.Add(roleCode);
        }

        AccountId = accountId;
        PublicId = publicId;
        Username = username;
        PasswordHash = passwordHash;
        SecurityStamp = securityStamp;
        IsActive = isActive;
        RoleCodes = copiedRoleCodes.AsReadOnly();
    }

    public override string ToString() {
        return nameof(AccountCredentialRecord);
    }
}
