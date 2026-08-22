namespace WebApi.Authorization;

using Microsoft.AspNetCore.Authorization;

// Resource-based: a controller must evaluate this after loading the owned resource.
internal sealed class AccountOwnershipAuthorizationRequirement : IAuthorizationRequirement { }
