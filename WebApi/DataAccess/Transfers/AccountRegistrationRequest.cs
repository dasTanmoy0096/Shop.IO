namespace DataAccess.Transfers;

public sealed record AccountRegistrationRequest(
    string Username,
    string Password
) {
    public override string ToString() {
        return nameof(AccountRegistrationRequest);
    }
}
