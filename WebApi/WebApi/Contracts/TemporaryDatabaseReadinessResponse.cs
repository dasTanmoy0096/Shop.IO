namespace WebApi.Contracts;

// TEMPORARY: Remove with the P3.07 readiness demonstration when P7 defines the real health API contract.
internal sealed record TemporaryDatabaseReadinessResponse {
    public bool IsReady { get; }

    internal TemporaryDatabaseReadinessResponse(bool isReady) {
        IsReady = isReady;
    }
}
