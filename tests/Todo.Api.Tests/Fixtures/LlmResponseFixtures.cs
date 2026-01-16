namespace Todo.Api.Tests.Fixtures;

/// <summary>
/// Centralized LLM response fixtures for all contract testing scenarios.
/// These fixtures represent various LLM response patterns that need to be handled
/// by the parsing and validation logic.
/// </summary>
public static class LlmResponseFixtures
{
    #region Story Root Fixtures

    /// <summary>
    /// Valid Story Root response - fully conforming JSON with all required fields.
    /// </summary>
    public const string ValidStoryRoot = @"{
  ""story_root_id"": ""story-12345"",
  ""genre"": ""Science Fiction"",
  ""tone"": ""Dark and Gritty"",
  ""thematic_pillars"": ""Exploration of AI consciousness, Loss of human connection, Technological dependence"",
  ""notes"": ""A dystopian future where AI and humans struggle for coexistence.""
}";

    /// <summary>
    /// Story Root response missing required field (genre).
    /// </summary>
    public const string StoryRootMissingRequiredField = @"{
  ""story_root_id"": ""story-12345"",
  ""tone"": ""Dark and Gritty"",
  ""thematic_pillars"": ""Exploration of AI consciousness"",
  ""notes"": ""A dystopian future.""
}";

    /// <summary>
    /// Story Root response with extra/unexpected fields.
    /// </summary>
    public const string StoryRootWithExtraFields = @"{
  ""story_root_id"": ""story-12345"",
  ""genre"": ""Science Fiction"",
  ""tone"": ""Dark and Gritty"",
  ""thematic_pillars"": ""Exploration of AI consciousness"",
  ""notes"": ""A dystopian future."",
  ""unexpected_field"": ""should be ignored"",
  ""another_extra"": 12345
}";

    /// <summary>
    /// Malformed Story Root JSON - invalid syntax.
    /// </summary>
    public const string StoryRootMalformedJson = @"{
  ""story_root_id"": ""story-12345"",
  ""genre"": ""Science Fiction"",
  ""tone"": ""Dark and Gritty"",
  ""thematic_pillars"": ""Exploration"",
  ""notes"": ""A dystopian future.""
  // Missing closing brace and trailing comma
}";

    /// <summary>
    /// Empty Story Root response.
    /// </summary>
    public const string StoryRootEmpty = @"{}";

    /// <summary>
    /// Story Root response with null values for optional fields.
    /// </summary>
    public const string StoryRootWithNulls = @"{
  ""story_root_id"": ""story-12345"",
  ""genre"": ""Science Fiction"",
  ""tone"": null,
  ""thematic_pillars"": ""Exploration"",
  ""notes"": null
}";

    #endregion

    #region World State Fixtures

    /// <summary>
    /// Valid World State response - fully conforming JSON with all required fields.
    /// </summary>
    public const string ValidWorldState = @"{
  ""world_state_id"": ""world-67890"",
  ""physical_laws"": ""Gravity works differently on each planet based on its mass. Time dilation occurs near black holes."",
  ""social_structures"": ""Interplanetary Federation with democratic representation. Corporate guilds control trade routes."",
  ""historical_context"": ""Humanity expanded into space after Earth's climate collapse in 2087. First contact with alien species occurred in 2150."",
  ""magic_or_technology"": ""Advanced quantum computing and terraforming technology. No magic system, purely technological."",
  ""notes"": ""A hard science fiction setting with focus on realism and scientific accuracy.""
}";

    /// <summary>
    /// World State response missing required field (physical_laws).
    /// </summary>
    public const string WorldStateMissingRequiredField = @"{
  ""world_state_id"": ""world-67890"",
  ""social_structures"": ""Interplanetary Federation"",
  ""historical_context"": ""Humanity expanded into space"",
  ""magic_or_technology"": ""Advanced quantum computing"",
  ""notes"": ""A hard science fiction setting.""
}";

    /// <summary>
    /// World State response with extra/unexpected fields.
    /// </summary>
    public const string WorldStateWithExtraFields = @"{
  ""world_state_id"": ""world-67890"",
  ""physical_laws"": ""Gravity works differently"",
  ""social_structures"": ""Interplanetary Federation"",
  ""historical_context"": ""Humanity expanded"",
  ""magic_or_technology"": ""Advanced technology"",
  ""notes"": ""A hard science fiction setting."",
  ""unexpected_field"": ""should be ignored"",
  ""random_data"": [1, 2, 3]
}";

    /// <summary>
    /// Malformed World State JSON - invalid syntax.
    /// </summary>
    public const string WorldStateMalformedJson = @"{
  ""world_state_id"": ""world-67890"",
  ""physical_laws"": ""Gravity works differently"",
  ""social_structures"": ""Interplanetary Federation"",
  // Missing required fields and invalid JSON structure
  ""notes"": ""A hard science fiction setting.""
}";

    /// <summary>
    /// Empty World State response.
    /// </summary>
    public const string WorldStateEmpty = @"{}";

    /// <summary>
    /// World State response with null values for optional fields.
    /// </summary>
    public const string WorldStateWithNulls = @"{
  ""world_state_id"": ""world-67890"",
  ""physical_laws"": ""Gravity works differently"",
  ""social_structures"": ""Interplanetary Federation"",
  ""historical_context"": null,
  ""magic_or_technology"": ""Advanced technology"",
  ""notes"": null
}";

    #endregion

    #region Edge Case Fixtures

    /// <summary>
    /// Null response string.
    /// </summary>
    public const string? NullResponse = null;

    /// <summary>
    /// Empty string response.
    /// </summary>
    public const string EmptyStringResponse = "";

    /// <summary>
    /// Whitespace-only response.
    /// </summary>
    public const string WhitespaceOnlyResponse = "   \n\t  ";

    /// <summary>
    /// Valid JSON but not a Story Root or World State structure.
    /// </summary>
    public const string ValidJsonWrongStructure = @"{
  ""message"": ""This is not a Story Root or World State"",
  ""data"": [1, 2, 3]
}";

    /// <summary>
    /// Response with only partial JSON (incomplete).
    /// </summary>
    public const string IncompleteJson = @"{
  ""story_root_id"": ""story-12345"",
  ""genre"": ""Science Fiction""";

    #endregion
}
