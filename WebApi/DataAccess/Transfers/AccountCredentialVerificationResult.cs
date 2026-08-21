namespace DataAccess.Transfers;

public sealed record AccountCredentialVerificationResult(
    AccountCredentialVerificationStatus Status,
    AuthenticatedAccount? Account
);
