namespace DataAccess.Transfers;

using System;
using System.Collections.Generic;

public sealed record AuthenticatedAccount {
    public Guid PublicId { get; }
    public string Username { get; }
    public Guid SecurityStamp { get; }
    public IReadOnlyList<string> RoleCodes { get; }

    public AuthenticatedAccount(
        Guid publicId,
        string username,
        Guid securityStamp,
        IEnumerable<string> roleCodes
    ) {
        ArgumentOutOfRangeException.ThrowIfEqual(publicId, Guid.Empty);
        ArgumentException.ThrowIfNullOrWhiteSpace(username);
        ArgumentOutOfRangeException.ThrowIfEqual(securityStamp, Guid.Empty);
        ArgumentNullException.ThrowIfNull(roleCodes);

        List<string> copiedRoleCodes = [];

        foreach (string roleCode in roleCodes) {
            ArgumentException.ThrowIfNullOrWhiteSpace(roleCode);
            copiedRoleCodes.Add(roleCode);
        }

        PublicId = publicId;
        Username = username;
        SecurityStamp = securityStamp;
        RoleCodes = copiedRoleCodes.AsReadOnly();
    }

    public override string ToString() {
        return nameof(AuthenticatedAccount);
    }
}
