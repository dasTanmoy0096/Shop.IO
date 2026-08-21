namespace DataAccess.Transfers;

using System;

public sealed record AuthenticatedAccount(
    Guid PublicId,
    string Username,
    Guid SecurityStamp
) {
    public override string ToString() {
        return nameof(AuthenticatedAccount);
    }
}
