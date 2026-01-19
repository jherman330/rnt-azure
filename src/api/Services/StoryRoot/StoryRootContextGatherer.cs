using System.Text.Json;
using SimpleTodo.Api.Models;

namespace SimpleTodo.Api.Services.StoryRoot;

/// <summary>
/// Helper class for gathering Story Root context from domain models.
/// Extracts relevant data for template variable preparation.
/// </summary>
internal static class StoryRootContextGatherer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = false
    };

    /// <summary>
    /// Formats a Story Root object as JSON string for template variable usage.
    /// Returns "null" if the Story Root is null.
    /// </summary>
    public static string FormatStoryRootForTemplate(Models.StoryRoot? storyRoot)
    {
        if (storyRoot == null)
        {
            return "null";
        }

        return JsonSerializer.Serialize(storyRoot, JsonOptions);
    }

    /// <summary>
    /// Extracts all relevant context from a Story Root for template usage.
    /// Returns a formatted string representation suitable for prompt templates.
    /// </summary>
    public static string GatherStoryRootContext(Models.StoryRoot? storyRoot)
    {
        return FormatStoryRootForTemplate(storyRoot);
    }
}
