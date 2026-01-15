using SimpleTodo.Api.Models;

namespace SimpleTodo.Api.Validation;

public class StoryRootValidator
{
    public (bool IsValid, string? ErrorMessage) Validate(StoryRoot storyRoot)
    {
        if (storyRoot == null)
        {
            return (false, "StoryRoot cannot be null");
        }

        if (string.IsNullOrWhiteSpace(storyRoot.StoryRootId))
        {
            return (false, "StoryRootId is required");
        }

        if (string.IsNullOrWhiteSpace(storyRoot.Genre))
        {
            return (false, "Genre is required");
        }

        if (string.IsNullOrWhiteSpace(storyRoot.Tone))
        {
            return (false, "Tone is required");
        }

        if (string.IsNullOrWhiteSpace(storyRoot.ThematicPillars))
        {
            return (false, "ThematicPillars is required");
        }

        // Notes is optional, so no validation needed

        // Validate JSON serialization
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(storyRoot);
            if (string.IsNullOrEmpty(json))
            {
                return (false, "StoryRoot cannot be serialized to JSON");
            }
        }
        catch (Exception ex)
        {
            return (false, $"StoryRoot JSON serialization failed: {ex.Message}");
        }

        return (true, null);
    }
}
