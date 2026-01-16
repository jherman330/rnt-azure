using System.Text.Json.Serialization;

namespace SimpleTodo.Api.Models;

/// <summary>
/// Response DTOs for Story Root and World State API operations.
/// </summary>

/// <summary>
/// Response containing a proposal and current version for comparison.
/// </summary>
public class ProposalResponse<T>
{
    [JsonPropertyName("proposal")]
    public T Proposal { get; set; } = default!;

    [JsonPropertyName("current")]
    public T? Current { get; set; }
}

/// <summary>
/// Response containing the newly created version information.
/// </summary>
public class CommitResponse<T>
{
    [JsonPropertyName("version_id")]
    public string VersionId { get; set; } = string.Empty;

    [JsonPropertyName("artifact")]
    public T Artifact { get; set; } = default!;
}

/// <summary>
/// Standard error response with correlation ID for tracing.
/// </summary>
public class ErrorResponse
{
    [JsonPropertyName("error")]
    public string Error { get; set; } = string.Empty;

    [JsonPropertyName("correlation_id")]
    public string CorrelationId { get; set; } = string.Empty;
}
