namespace WebApi.Authorization;

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

internal sealed class AccountOwnershipAuthorizationHandler : AuthorizationHandler<AccountOwnershipAuthorizationRequirement, AccountOwnershipAuthorizationResource> {
    public AccountOwnershipAuthorizationHandler() { }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccountOwnershipAuthorizationRequirement requirement,
        AccountOwnershipAuthorizationResource resource
    ) {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);
        ArgumentNullException.ThrowIfNull(resource);

        if (AccountAuthorizationClaimReader.TryRead(
            context.User,
            out _,
            out Guid accountPublicId
        ) && accountPublicId == resource.OwnerAccountPublicId) {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
