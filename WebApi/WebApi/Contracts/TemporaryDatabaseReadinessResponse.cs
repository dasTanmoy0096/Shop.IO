namespace WebApi.Contracts;

// TEMPORARY: Remove with the P3.07 readiness demonstration when P7 defines the real health API contract.
public sealed record TemporaryDatabaseReadinessResponse(bool IsReady);
