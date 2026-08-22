namespace WebApi.Authorization;

using System;
using System.Security.Claims;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

internal sealed class AccountRoleAuthorizationHandler : AuthorizationHandler<AccountRoleAuthorizationRequirement> {
    public AccountRoleAuthorizationHandler() { }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AccountRoleAuthorizationRequirement requirement
    ) {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        if (AccountAuthorizationClaimReader.TryRead(
            context.User,
            out ClaimsIdentity? accountIdentity,
            out _
        ) && accountIdentity is not null
            && AccountAuthorizationClaimReader.HasRole(
                accountIdentity,
                requirement.RoleCode
        )) {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
