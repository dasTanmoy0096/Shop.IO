namespace WebApi.Contracts;

using System.Text.Json.Serialization;

// TEMPORARY: Remove with the P3.07 readiness demonstration when P7 defines the real health API contract.
internal sealed record TemporaryDatabaseReadinessResponse {
    [JsonPropertyName("isReady")]
    [JsonPropertyOrder(0)]
    public bool IsReady { get; }

    internal TemporaryDatabaseReadinessResponse(bool isReady) {
        IsReady = isReady;
    }
}
