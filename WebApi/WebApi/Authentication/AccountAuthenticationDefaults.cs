namespace WebApi.Authentication;

internal static class AccountAuthenticationDefaults {
    internal const string Scheme = "ShopIO.Cookie";
    internal const string ClaimIssuer = "Shop.IO Web API";
    internal const string SecurityStampClaimType = "urn:shopio:security-stamp";
    internal const string StartupGateProtectorPurpose = "Shop.IO.WebApi.Authentication.StartupGate";
}
