namespace DataAccess.Repositories;

using System;

using DataAccess.Configuration;

internal sealed class NewAccountRecord {
    internal Guid PublicId { get; }
    internal AccountUsername Username { get; }
    internal string PasswordHash { get; }
    internal Guid SecurityStamp { get; }

    internal NewAccountRecord(
        Guid publicId,
        AccountUsername username,
        string passwordHash,
        Guid securityStamp
    ) {
        ArgumentNullException.ThrowIfNull(username);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

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

        PublicId = publicId;
        Username = username;
        PasswordHash = passwordHash;
        SecurityStamp = securityStamp;
    }

    public override string ToString() {
        return nameof(NewAccountRecord);
    }
}
