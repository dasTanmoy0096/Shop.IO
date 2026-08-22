namespace WebApi.Authorization;

using System;
using System.Security.Claims;

using WebApi.Authentication;

internal static class AccountAuthorizationClaimReader {
    internal static bool TryRead(
        ClaimsPrincipal user,
        out ClaimsIdentity? accountIdentity,
        out Guid accountPublicId
    ) {
        ArgumentNullException.ThrowIfNull(user);

        accountIdentity = null;
        accountPublicId = Guid.Empty;
        ClaimsIdentity? candidateIdentity = null;

        foreach (ClaimsIdentity identity in user.Identities) {
            if (!identity.IsAuthenticated
                || !string.Equals(
                    identity.AuthenticationType,
                    AccountAuthenticationDefaults.Scheme,
                    StringComparison.Ordinal
            )) {
                continue;
            }

            if (candidateIdentity is not null) {
                return false;
            }

            candidateIdentity = identity;
        }

        if (candidateIdentity is null
            || !TryReadExactGuidClaim(
                candidateIdentity,
                ClaimTypes.NameIdentifier,
                out accountPublicId
            ) || !TryReadExactGuidClaim(
                candidateIdentity,
                AccountAuthenticationDefaults.SecurityStampClaimType,
                out _
        )) {
            accountPublicId = Guid.Empty;
            return false;
        }

        accountIdentity = candidateIdentity;

        return true;
    }

    internal static bool HasRole(
        ClaimsIdentity accountIdentity,
        string roleCode
    ) {
        ArgumentNullException.ThrowIfNull(accountIdentity);
        ArgumentException.ThrowIfNullOrWhiteSpace(roleCode);

        foreach (Claim roleClaim in accountIdentity.FindAll(ClaimTypes.Role)) {
            if (string.Equals(
                roleClaim.Issuer,
                AccountAuthenticationDefaults.ClaimIssuer,
                StringComparison.Ordinal
            ) && string.Equals(
                roleClaim.Value,
                roleCode,
                StringComparison.Ordinal
            )) {
                return true;
            }
        }

        return false;
    }

    private static bool TryReadExactGuidClaim(
        ClaimsIdentity accountIdentity,
        string claimType,
        out Guid claimValue
    ) {
        claimValue = Guid.Empty;
        bool wasFound = false;

        foreach (Claim claim in accountIdentity.FindAll(claimType)) {
            if (!string.Equals(
                claim.Issuer,
                AccountAuthenticationDefaults.ClaimIssuer,
                StringComparison.Ordinal
            ) || wasFound
                || !Guid.TryParseExact(
                    claim.Value,
                    "D",
                    out Guid parsedValue
            ) || parsedValue == Guid.Empty) {
                return false;
            }

            claimValue = parsedValue;
            wasFound = true;
        }

        return wasFound;
    }
}
