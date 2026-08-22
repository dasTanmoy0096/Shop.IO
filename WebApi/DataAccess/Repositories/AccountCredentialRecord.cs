namespace DataAccess.Repositories;

using System;
using System.Collections.Generic;

internal sealed class AccountCredentialRecord {
    internal long AccountId { get; }
    internal Guid PublicId { get; }
    internal string Username { get; }
    internal string PasswordHash { get; }
    internal Guid SecurityStamp { get; }
    internal bool IsActive { get; }
    internal IReadOnlyList<string> RoleCodes { get; }

    internal AccountCredentialRecord(
        long accountId,
        Guid publicId,
        string username,
        string passwordHash,
        Guid securityStamp,
        bool isActive,
        IEnumerable<string> roleCodes
    ) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(accountId);
        ArgumentOutOfRangeException.ThrowIfEqual(publicId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);
        ArgumentOutOfRangeException.ThrowIfEqual(securityStamp, Guid.Empty);
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
