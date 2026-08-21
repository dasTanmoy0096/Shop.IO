namespace WebApi.Authentication;

using System;
using System.Security.Claims;
using System.Threading.Tasks;

using DataAccess.Services;
using DataAccess.Transfers;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

internal sealed class AccountCookieAuthenticationEvents : CookieAuthenticationEvents {
    private readonly IAccountService accountService;
    private readonly ILogger<AccountCookieAuthenticationEvents> logger;

    public AccountCookieAuthenticationEvents(
        IAccountService accountService,
        ILogger<AccountCookieAuthenticationEvents> logger
    ) {
        ArgumentNullException.ThrowIfNull(accountService);
        ArgumentNullException.ThrowIfNull(logger);

        this.accountService = accountService;
        this.logger = logger;
    }

    public override async Task ValidatePrincipal(
        CookieValidatePrincipalContext context
    ) {
        ArgumentNullException.ThrowIfNull(context);

        if (!TryReadSessionClaims(
            context.Principal,
            out Guid accountPublicId,
            out Guid securityStamp
        )) {
            await RejectAndSignOutAsync(context);
            return;
        }

        AccountSessionValidationResult validationResult = await accountService.ValidateSessionAsync(
            new AccountSessionValidationRequest(
                accountPublicId,
                securityStamp
            ),
            context.HttpContext.RequestAborted
        );

        switch (validationResult.Status) {
            case AccountSessionValidationStatus.Valid:
                return;
            case AccountSessionValidationStatus.Invalid:
                await RejectAndSignOutAsync(context);
                return;
            case AccountSessionValidationStatus.Unavailable:
                logger.LogWarning("Cookie session validation was unavailable; the request was treated as unauthenticated.");
                context.RejectPrincipal();
                return;
            default:
                throw new InvalidOperationException("The account-session validation result has an unsupported status.");
        }
    }

    public override Task RedirectToLogin(
        RedirectContext<CookieAuthenticationOptions> context
    ) {
        ArgumentNullException.ThrowIfNull(context);

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;

        return Task.CompletedTask;
    }

    public override Task RedirectToAccessDenied(
        RedirectContext<CookieAuthenticationOptions> context
    ) {
        ArgumentNullException.ThrowIfNull(context);

        context.Response.StatusCode = StatusCodes.Status403Forbidden;

        return Task.CompletedTask;
    }

    private static bool TryReadSessionClaims(
        ClaimsPrincipal? principal,
        out Guid accountPublicId,
        out Guid securityStamp
    ) {
        accountPublicId = Guid.Empty;
        securityStamp = Guid.Empty;

        if (principal?.Identity is not ClaimsIdentity identity
            || !identity.IsAuthenticated
            || !string.Equals(
                identity.AuthenticationType,
                AccountAuthenticationDefaults.Scheme,
                StringComparison.Ordinal
        )) {
            return false;
        }

        Claim? accountPublicIdClaim = identity.FindFirst(ClaimTypes.NameIdentifier);
        Claim? securityStampClaim = identity.FindFirst(AccountAuthenticationDefaults.SecurityStampClaimType);

        return accountPublicIdClaim is not null
            && securityStampClaim is not null
            && Guid.TryParseExact(
                accountPublicIdClaim.Value,
                "D",
                out accountPublicId
            )
            && Guid.TryParseExact(
                securityStampClaim.Value,
                "D",
                out securityStamp
            )
            && accountPublicId != Guid.Empty
            && securityStamp != Guid.Empty;
    }

    private static async Task RejectAndSignOutAsync(
        CookieValidatePrincipalContext context
    ) {
        context.RejectPrincipal();

        await context.HttpContext.SignOutAsync(AccountAuthenticationDefaults.Scheme);
    }
}
