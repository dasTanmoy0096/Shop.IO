namespace WebApi.Authentication;

using System;
using System.Collections.Generic;
using System.Security.Claims;

using DataAccess.Transfers;

using Microsoft.AspNetCore.Authentication;

internal sealed class AccountCookieTicketFactory {
    private readonly WebApiAuthenticationConfiguration authenticationConfiguration;
    private readonly TimeProvider timeProvider;

    public AccountCookieTicketFactory(
        WebApiAuthenticationConfiguration authenticationConfiguration,
        TimeProvider timeProvider
    ) {
        ArgumentNullException.ThrowIfNull(authenticationConfiguration);
        ArgumentNullException.ThrowIfNull(timeProvider);

        this.authenticationConfiguration = authenticationConfiguration;
        this.timeProvider = timeProvider;
    }

    internal AuthenticationTicket Create(AuthenticatedAccount account) {
        ArgumentNullException.ThrowIfNull(account);

        DateTimeOffset issuedUtc = timeProvider.GetUtcNow();
        AuthenticationProperties properties = new() {
            IsPersistent = authenticationConfiguration.CookiePersistent,
            AllowRefresh = authenticationConfiguration.SlidingExpiration,
            IssuedUtc = issuedUtc,
            ExpiresUtc = issuedUtc.Add(authenticationConfiguration.CookieLifetime),
        };

        return new AuthenticationTicket(
            CreatePrincipal(account),
            properties,
            AccountAuthenticationDefaults.Scheme
        );
    }

    private static ClaimsPrincipal CreatePrincipal(AuthenticatedAccount account) {
        if (account.PublicId == Guid.Empty) {
            throw new ArgumentOutOfRangeException(
                nameof(account),
                "The authenticated account public identifier must not be empty."
            );
        }

        if (account.SecurityStamp == Guid.Empty) {
            throw new ArgumentOutOfRangeException(
                nameof(account),
                "The authenticated account security stamp must not be empty."
            );
        }

        List<Claim> claims = [
            new Claim(
                ClaimTypes.NameIdentifier,
                account.PublicId.ToString("D"),
                ClaimValueTypes.String,
                AccountAuthenticationDefaults.ClaimIssuer
            ),
            new Claim(
                AccountAuthenticationDefaults.SecurityStampClaimType,
                account.SecurityStamp.ToString("D"),
                ClaimValueTypes.String,
                AccountAuthenticationDefaults.ClaimIssuer
            ),
        ];

        foreach (string roleCode in account.RoleCodes) {
            claims.Add(new Claim(
                ClaimTypes.Role,
                roleCode,
                ClaimValueTypes.String,
                AccountAuthenticationDefaults.ClaimIssuer
            ));
        }

        ClaimsIdentity identity = new(
            claims,
            AccountAuthenticationDefaults.Scheme,
            ClaimTypes.NameIdentifier,
            ClaimTypes.Role
        );

        return new ClaimsPrincipal(identity);
    }
}
