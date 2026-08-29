namespace WebApi.Middleware;

using System;
using System.Text.Json.Serialization;

internal sealed class WebApiProblemDetailsResponse {
    [JsonPropertyName("type")]
    [JsonPropertyOrder(0)]
    public string? Type { get; }

    [JsonPropertyName("title")]
    [JsonPropertyOrder(1)]
    public string? Title { get; }

    [JsonPropertyName("status")]
    [JsonPropertyOrder(2)]
    public int Status { get; }

    [JsonPropertyName("detail")]
    [JsonPropertyOrder(3)]
    public string? Detail { get; }

    [JsonPropertyName("instance")]
    [JsonPropertyOrder(4)]
    public string? Instance { get; }

    [JsonPropertyName("requestId")]
    [JsonPropertyOrder(5)]
    public string RequestId { get; }

    internal WebApiProblemDetailsResponse(
        string? type,
        string? title,
        int status,
        string? detail,
        string? instance,
        string requestId
    ) {
        ArgumentNullException.ThrowIfNull(requestId);

        Type = type;
        Title = title;
        Status = status;
        Detail = detail;
        Instance = instance;
        RequestId = requestId;
    }
}
