namespace SimpleTodo.Api.Utils;

public static class ArtifactPathHelper
{
    private const string StoryRootArtifactType = "story-root";
    private const string WorldStateArtifactType = "world-state";
    private const string RootPathSegment = "root";
    private const string WorldPathSegment = "world";
    private const string VersionsPathSegment = "versions";
    private const string CurrentFileName = "current.json";

    public static string GetStoryRootVersionPath(string userId, string versionId)
    {
        return $"users/{userId}/{StoryRootArtifactType}/{RootPathSegment}/{VersionsPathSegment}/{versionId}.json";
    }

    public static string GetWorldStateVersionPath(string userId, string versionId)
    {
        return $"users/{userId}/{WorldStateArtifactType}/{WorldPathSegment}/{VersionsPathSegment}/{versionId}.json";
    }

    public static string GetStoryRootCurrentPath(string userId)
    {
        return $"users/{userId}/{StoryRootArtifactType}/{RootPathSegment}/{CurrentFileName}";
    }

    public static string GetWorldStateCurrentPath(string userId)
    {
        return $"users/{userId}/{WorldStateArtifactType}/{WorldPathSegment}/{CurrentFileName}";
    }
}
