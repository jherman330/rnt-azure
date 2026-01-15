using System.Text.Json.Serialization;

namespace SimpleTodo.Api.Models;

public class StoryRoot
{
    [JsonPropertyName("story_root_id")]
    public string StoryRootId { get; set; } = string.Empty;

    [JsonPropertyName("genre")]
    public string Genre { get; set; } = string.Empty;

    [JsonPropertyName("tone")]
    public string Tone { get; set; } = string.Empty;

    [JsonPropertyName("thematic_pillars")]
    public string ThematicPillars { get; set; } = string.Empty;

    [JsonPropertyName("notes")]
    public string Notes { get; set; } = string.Empty;
}
