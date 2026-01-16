namespace SimpleTodo.Api.Services;

/// <summary>
/// Implementation of IPromptTemplateService for retrieving version-controlled prompt templates.
/// For Phase-0, this uses hardcoded templates. In future phases, this could be enhanced
/// to load templates from storage or a configuration system.
/// </summary>
public class PromptTemplateService : IPromptTemplateService
{
    public Task<string> GetPromptTemplateAsync(string templateName, string version)
    {
        // For Phase-0, return hardcoded templates based on template name and version
        // In future phases, this could load from blob storage, database, or configuration
        
        var template = templateName.ToLowerInvariant() switch
        {
            "story_root_merge" => GetStoryRootMergeTemplate(version),
            "world_state_merge" => GetWorldStateMergeTemplate(version),
            _ => throw new ArgumentException($"Unknown template name: {templateName}", nameof(templateName))
        };

        return Task.FromResult(template);
    }

    private string GetStoryRootMergeTemplate(string version)
    {
        // For Phase-0, use a simple hardcoded template
        // Version "1.0" is the default/current version
        return version switch
        {
            "1.0" or "" => @"You are a narrative assistant helping to maintain a Story Root document.

The Story Root defines the foundational elements of a narrative:
- genre: The genre of the story
- tone: The emotional tone and atmosphere
- thematic_pillars: Core themes and messages
- notes: Additional context and notes

Current Story Root (if exists):
{current_story_root}

User Input:
{user_input}

Merge the user input into the Story Root, preserving existing content while incorporating the new information. Return ONLY a valid JSON object matching this structure:
{
  ""story_root_id"": ""<existing or new id>"",
  ""genre"": ""<genre>"",
  ""tone"": ""<tone>"",
  ""thematic_pillars"": ""<thematic pillars>"",
  ""notes"": ""<notes>""
}

Do not include any explanatory text, only the JSON object.",
            _ => throw new ArgumentException($"Unknown template version: {version} for story_root_merge", nameof(version))
        };
    }

    private string GetWorldStateMergeTemplate(string version)
    {
        // For Phase-0, use a simple hardcoded template
        // Version "1.0" is the default/current version
        return version switch
        {
            "1.0" or "" => @"You are a narrative assistant helping to maintain a World State document.

The World State defines the foundational elements of a story's setting:
- physical_laws: The physical laws and rules of the world
- social_structures: Social organization and structures
- historical_context: Historical background and events
- magic_or_technology: Magic systems or technology level
- notes: Additional context and notes

Current World State (if exists):
{current_world_state}

User Input:
{user_input}

Merge the user input into the World State, preserving existing content while incorporating the new information. Return ONLY a valid JSON object matching this structure:
{
  ""world_state_id"": ""<existing or new id>"",
  ""physical_laws"": ""<physical laws>"",
  ""social_structures"": ""<social structures>"",
  ""historical_context"": ""<historical context>"",
  ""magic_or_technology"": ""<magic or technology>"",
  ""notes"": ""<notes>""
}

Do not include any explanatory text, only the JSON object.",
            _ => throw new ArgumentException($"Unknown template version: {version} for world_state_merge", nameof(version))
        };
    }
}
