using SimpleTodo.Api.Models;
using Todo.Api.Tests.TestUtilities;

namespace Todo.Api.Tests.Fixtures;

/// <summary>
/// Predefined VersionMetadata objects for provenance testing scenarios.
/// </summary>
public static class MetadataFixtures
{
    /// <summary>
    /// Initial version metadata - no prior version, first artifact creation.
    /// </summary>
    public static VersionMetadata InitialVersion => new()
    {
        VersionId = TestConstants.TestVersionId,
        UserId = TestConstants.TestUserId,
        Timestamp = DateTimeOffset.UtcNow.AddHours(-2),
        SourceRequestId = TestConstants.TestSourceRequestId,
        PriorVersionId = null,
        Environment = TestConstants.TestEnvironment,
        LlmAssisted = false
    };

    /// <summary>
    /// Updated version metadata - links to prior version.
    /// </summary>
    public static VersionMetadata UpdatedVersion => new()
    {
        VersionId = Guid.NewGuid().ToString("N"),
        UserId = TestConstants.TestUserId,
        Timestamp = DateTimeOffset.UtcNow.AddHours(-1),
        SourceRequestId = Guid.NewGuid().ToString("N"),
        PriorVersionId = TestConstants.TestVersionId,
        Environment = TestConstants.TestEnvironment,
        LlmAssisted = true
    };

    /// <summary>
    /// LLM-assisted version metadata.
    /// </summary>
    public static VersionMetadata LlmAssistedVersion => new()
    {
        VersionId = Guid.NewGuid().ToString("N"),
        UserId = TestConstants.TestUserId,
        Timestamp = DateTimeOffset.UtcNow,
        SourceRequestId = TestConstants.TestSourceRequestId,
        PriorVersionId = TestConstants.TestPriorVersionId,
        Environment = TestConstants.TestEnvironment,
        LlmAssisted = true
    };

    /// <summary>
    /// Non-LLM-assisted version metadata (manual update).
    /// </summary>
    public static VersionMetadata ManualVersion => new()
    {
        VersionId = Guid.NewGuid().ToString("N"),
        UserId = TestConstants.TestUserId,
        Timestamp = DateTimeOffset.UtcNow,
        SourceRequestId = TestConstants.TestSourceRequestId,
        PriorVersionId = TestConstants.TestPriorVersionId,
        Environment = TestConstants.TestEnvironment,
        LlmAssisted = false
    };

    /// <summary>
    /// Version metadata for a different user.
    /// </summary>
    public static VersionMetadata DifferentUserVersion => new()
    {
        VersionId = Guid.NewGuid().ToString("N"),
        UserId = TestConstants.TestUserId2,
        Timestamp = DateTimeOffset.UtcNow,
        SourceRequestId = Guid.NewGuid().ToString("N"),
        PriorVersionId = null,
        Environment = TestConstants.TestEnvironment,
        LlmAssisted = false
    };

    /// <summary>
    /// Version metadata for production environment.
    /// </summary>
    public static VersionMetadata ProductionVersion => new()
    {
        VersionId = Guid.NewGuid().ToString("N"),
        UserId = TestConstants.TestUserId,
        Timestamp = DateTimeOffset.UtcNow,
        SourceRequestId = TestConstants.TestSourceRequestId,
        PriorVersionId = TestConstants.TestVersionId,
        Environment = TestConstants.ProductionEnvironment,
        LlmAssisted = true
    };

    /// <summary>
    /// Creates a version metadata with a specific version ID and prior version ID for version linking tests.
    /// </summary>
    public static VersionMetadata CreateVersionWithLink(string versionId, string? priorVersionId = null) => new()
    {
        VersionId = versionId,
        UserId = TestConstants.TestUserId,
        Timestamp = DateTimeOffset.UtcNow,
        SourceRequestId = TestConstants.TestSourceRequestId,
        PriorVersionId = priorVersionId,
        Environment = TestConstants.TestEnvironment,
        LlmAssisted = priorVersionId != null
    };
}
