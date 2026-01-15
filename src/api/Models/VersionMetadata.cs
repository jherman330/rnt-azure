using System.Text.Json.Serialization;

namespace SimpleTodo.Api.Models;

public class VersionMetadata
{
    [JsonPropertyName("version_id")]
    public string VersionId { get; set; } = string.Empty;

    [JsonPropertyName("user_id")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    [JsonPropertyName("source_request_id")]
    public string? SourceRequestId { get; set; }

    [JsonPropertyName("prior_version_id")]
    public string? PriorVersionId { get; set; }

    [JsonPropertyName("environment")]
    public string? Environment { get; set; }

    [JsonPropertyName("llm_assisted")]
    public bool LlmAssisted { get; set; }
}
