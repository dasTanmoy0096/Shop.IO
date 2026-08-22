namespace WebApi.Authorization;

using System;

using Microsoft.AspNetCore.Authorization;

internal sealed class AccountRoleAuthorizationRequirement : IAuthorizationRequirement {
    internal string RoleCode { get; }

    internal AccountRoleAuthorizationRequirement(string roleCode) {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleCode);

        RoleCode = roleCode;
    }
}
