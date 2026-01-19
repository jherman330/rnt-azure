using System.Text.Json.Serialization;

namespace SimpleTodo.Api.Models;

/// <summary>
/// Request DTOs for Story Root and World State API operations.
/// </summary>

/// <summary>
/// Request for proposing a merge of raw input into Story Root.
/// </summary>
public class ProposeStoryRootMergeRequest
{
    [JsonPropertyName("raw_input")]
    public string RawInput { get; set; } = string.Empty;
}

/// <summary>
/// Request for proposing a merge of raw input into World State.
/// </summary>
public class ProposeWorldStateMergeRequest
{
    [JsonPropertyName("raw_input")]
    public string RawInput { get; set; } = string.Empty;
}

/// <summary>
/// Request for committing a Story Root proposal as a new version.
/// </summary>
public class CommitStoryRootRequest
{
    [JsonPropertyName("story_root")]
    public StoryRoot StoryRoot { get; set; } = null!;

    [JsonPropertyName("expected_version_id")]
    public string? ExpectedVersionId { get; set; }
}

/// <summary>
/// Request for committing a World State proposal as a new version.
/// </summary>
public class CommitWorldStateRequest
{
    [JsonPropertyName("world_state")]
    public WorldState WorldState { get; set; } = null!;
}
