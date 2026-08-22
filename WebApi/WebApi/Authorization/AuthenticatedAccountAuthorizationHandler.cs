namespace WebApi.Authorization;

using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Authorization;

internal sealed class AuthenticatedAccountAuthorizationHandler : AuthorizationHandler<AuthenticatedAccountAuthorizationRequirement> {
    public AuthenticatedAccountAuthorizationHandler() { }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        AuthenticatedAccountAuthorizationRequirement requirement
    ) {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(requirement);

        if (AccountAuthorizationClaimReader.TryRead(
            context.User,
            out _,
            out _
        )) {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
