using SimpleTodo.Api.Models;
using SimpleTodo.Api.Services;
using SimpleTodo.Api.Utils;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SimpleTodo.Api.Repositories;

public class StoryRootRepository : IStoryRootRepository
{
    private readonly IBlobArtifactRepository _blobRepository;
    private readonly IUserContextService _userContextService;
    private readonly JsonSerializerOptions _jsonOptions;

    public StoryRootRepository(IBlobArtifactRepository blobRepository, IUserContextService userContextService)
    {
        _blobRepository = blobRepository;
        _userContextService = userContextService;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };
    }

    public async Task<string> SaveNewVersionAsync(StoryRoot storyRoot, string? priorVersionId = null, string? sourceRequestId = null, string? environment = null, bool llmAssisted = false)
    {
        var userId = _userContextService.GetCurrentUserId();
        var versionId = Guid.NewGuid().ToString("N");

        var metadata = new VersionMetadata
        {
            VersionId = versionId,
            UserId = userId,
            Timestamp = DateTimeOffset.UtcNow,
            SourceRequestId = sourceRequestId,
            PriorVersionId = priorVersionId,
            Environment = environment,
            LlmAssisted = llmAssisted
        };

        var versionedArtifact = new
        {
            version_metadata = metadata,
            story_root = storyRoot
        };

        var jsonContent = JsonSerializer.Serialize(versionedArtifact, _jsonOptions);
        var blobPath = ArtifactPathHelper.GetStoryRootVersionPath(userId, versionId);

        await _blobRepository.SaveBlobAsync(blobPath, jsonContent);

        // Update current version pointer
        var currentVersionContent = JsonSerializer.Serialize(new { version_id = versionId }, _jsonOptions);
        var currentPath = ArtifactPathHelper.GetStoryRootCurrentPath(userId);
        await _blobRepository.SaveBlobAsync(currentPath, currentVersionContent);

        return versionId;
    }

    public async Task<StoryRoot?> GetCurrentVersionAsync(string userId)
    {
        var currentPath = ArtifactPathHelper.GetStoryRootCurrentPath(userId);
        var currentContent = await _blobRepository.GetBlobAsync(currentPath);

        if (string.IsNullOrEmpty(currentContent))
        {
            return null;
        }

        var currentVersion = JsonSerializer.Deserialize<CurrentVersionPointer>(currentContent, _jsonOptions);
        if (currentVersion == null || string.IsNullOrEmpty(currentVersion.VersionId))
        {
            return null;
        }

        return await GetVersionAsync(userId, currentVersion.VersionId);
    }

    public async Task<StoryRoot?> GetVersionAsync(string userId, string versionId)
    {
        var blobPath = ArtifactPathHelper.GetStoryRootVersionPath(userId, versionId);
        var jsonContent = await _blobRepository.GetBlobAsync(blobPath);

        if (string.IsNullOrEmpty(jsonContent))
        {
            return null;
        }

        var versionedArtifact = JsonSerializer.Deserialize<VersionedStoryRoot>(jsonContent, _jsonOptions);
        return versionedArtifact?.StoryRoot;
    }

    public async Task<List<VersionMetadata>> ListVersionsAsync(string userId)
    {
        var prefix = $"users/{userId}/story-root/root/versions/";
        var blobNames = await _blobRepository.ListBlobsAsync(prefix);

        var versions = new List<VersionMetadata>();

        foreach (var blobName in blobNames)
        {
            var jsonContent = await _blobRepository.GetBlobAsync(blobName);
            if (!string.IsNullOrEmpty(jsonContent))
            {
                var versionedArtifact = JsonSerializer.Deserialize<VersionedStoryRoot>(jsonContent, _jsonOptions);
                if (versionedArtifact?.VersionMetadata != null)
                {
                    versions.Add(versionedArtifact.VersionMetadata);
                }
            }
        }

        return versions.OrderByDescending(v => v.Timestamp).ToList();
    }

    private class CurrentVersionPointer
    {
        public string VersionId { get; set; } = string.Empty;
    }

    private class VersionedStoryRoot
    {
        [JsonPropertyName("version_metadata")]
        public VersionMetadata? VersionMetadata { get; set; }

        [JsonPropertyName("story_root")]
        public StoryRoot? StoryRoot { get; set; }
    }
}
