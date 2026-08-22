namespace WebApi.Authorization;

using System;

internal sealed record AccountOwnershipAuthorizationResource {
    internal Guid OwnerAccountPublicId { get; }

    internal AccountOwnershipAuthorizationResource(Guid ownerAccountPublicId) {
        ArgumentOutOfRangeException.ThrowIfEqual(ownerAccountPublicId, Guid.Empty);

        OwnerAccountPublicId = ownerAccountPublicId;
    }
}
