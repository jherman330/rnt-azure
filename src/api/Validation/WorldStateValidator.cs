using SimpleTodo.Api.Models;

namespace SimpleTodo.Api.Validation;

public class WorldStateValidator
{
    public (bool IsValid, string? ErrorMessage) Validate(WorldState worldState)
    {
        if (worldState == null)
        {
            return (false, "WorldState cannot be null");
        }

        if (string.IsNullOrWhiteSpace(worldState.WorldStateId))
        {
            return (false, "WorldStateId is required");
        }

        if (string.IsNullOrWhiteSpace(worldState.PhysicalLaws))
        {
            return (false, "PhysicalLaws is required");
        }

        if (string.IsNullOrWhiteSpace(worldState.SocialStructures))
        {
            return (false, "SocialStructures is required");
        }

        if (string.IsNullOrWhiteSpace(worldState.HistoricalContext))
        {
            return (false, "HistoricalContext is required");
        }

        if (string.IsNullOrWhiteSpace(worldState.MagicOrTechnology))
        {
            return (false, "MagicOrTechnology is required");
        }

        // Notes is optional, so no validation needed

        // Validate JSON serialization
        try
        {
            var json = System.Text.Json.JsonSerializer.Serialize(worldState);
            if (string.IsNullOrEmpty(json))
            {
                return (false, "WorldState cannot be serialized to JSON");
            }
        }
        catch (Exception ex)
        {
            return (false, $"WorldState JSON serialization failed: {ex.Message}");
        }

        return (true, null);
    }
}
