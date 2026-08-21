namespace DataAccess.Transfers;

public sealed record AccountCredentialVerificationRequest(
    string Username,
    string Password
) {
    public override string ToString() {
        return nameof(AccountCredentialVerificationRequest);
    }
}
