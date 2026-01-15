using SimpleTodo.Api.Models;

namespace SimpleTodo.Api.Repositories;

public interface IWorldStateRepository
{
    Task<string> SaveNewVersionAsync(WorldState worldState, string? priorVersionId = null, string? sourceRequestId = null, string? environment = null, bool llmAssisted = false);
    Task<WorldState?> GetCurrentVersionAsync(string userId);
    Task<WorldState?> GetVersionAsync(string userId, string versionId);
    Task<List<VersionMetadata>> ListVersionsAsync(string userId);
}
