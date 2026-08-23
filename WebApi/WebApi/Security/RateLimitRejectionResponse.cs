namespace WebApi.Security;

using System.Text.Json.Serialization;

internal sealed record RateLimitRejectionResponse {
    [JsonPropertyName("message")]
    public string Message { get; }

    internal RateLimitRejectionResponse(string message) {
        Message = message;
    }
}
