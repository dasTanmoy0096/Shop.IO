namespace DataAccess.Transfers;

public sealed record AccountRegistrationResult(
    AccountRegistrationStatus Status,
    AccountIdentity? Account
);
