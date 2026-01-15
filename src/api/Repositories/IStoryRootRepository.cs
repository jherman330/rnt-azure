using SimpleTodo.Api.Models;

namespace SimpleTodo.Api.Repositories;

public interface IStoryRootRepository
{
    Task<string> SaveNewVersionAsync(StoryRoot storyRoot, string? priorVersionId = null, string? sourceRequestId = null, string? environment = null, bool llmAssisted = false);
    Task<StoryRoot?> GetCurrentVersionAsync(string userId);
    Task<StoryRoot?> GetVersionAsync(string userId, string versionId);
    Task<List<VersionMetadata>> ListVersionsAsync(string userId);
}
