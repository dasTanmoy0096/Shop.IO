namespace DataAccess.Services;

using System.Threading;
using System.Threading.Tasks;

using DataAccess.Transfers;

public interface IAccountService {
    Task<AccountRegistrationResult> RegisterAsync(
        AccountRegistrationRequest request,
        CancellationToken cancellationToken
    );

    Task<AccountCredentialVerificationResult> VerifyCredentialsAsync(
        AccountCredentialVerificationRequest request,
        CancellationToken cancellationToken
    );

    Task<AccountSessionInvalidationResult> InvalidateSessionsAsync(
        AccountSessionInvalidationRequest request,
        CancellationToken cancellationToken
    );
}
