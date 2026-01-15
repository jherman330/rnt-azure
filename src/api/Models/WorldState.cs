using System.Text.Json.Serialization;

namespace SimpleTodo.Api.Models;

public class WorldState
{
    [JsonPropertyName("world_state_id")]
    public string WorldStateId { get; set; } = string.Empty;

    [JsonPropertyName("physical_laws")]
    public string PhysicalLaws { get; set; } = string.Empty;

    [JsonPropertyName("social_structures")]
    public string SocialStructures { get; set; } = string.Empty;

    [JsonPropertyName("historical_context")]
    public string HistoricalContext { get; set; } = string.Empty;

    [JsonPropertyName("magic_or_technology")]
    public string MagicOrTechnology { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;
}
