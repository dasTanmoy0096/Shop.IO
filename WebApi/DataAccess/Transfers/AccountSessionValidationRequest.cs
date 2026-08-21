namespace DataAccess.Transfers;

using System;

public sealed record AccountSessionValidationRequest(
    Guid AccountPublicId,
    Guid SecurityStamp
) {
    public override string ToString() {
        return nameof(AccountSessionValidationRequest);
    }
}
