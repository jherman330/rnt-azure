namespace Todo.Api.Tests.TestUtilities;

/// <summary>
/// Common test constants for use across all test suites.
/// </summary>
public static class TestConstants
{
    /// <summary>
    /// Standard test user ID.
    /// </summary>
    public const string TestUserId = "test-user-12345";

    /// <summary>
    /// Alternative test user ID for multi-user scenarios.
    /// </summary>
    public const string TestUserId2 = "test-user-67890";

    /// <summary>
    /// Standard test version ID.
    /// </summary>
    public const string TestVersionId = "a1b2c3d4e5f6789012345678901234ab";

    /// <summary>
    /// Alternative test version ID for version linking scenarios.
    /// </summary>
    public const string TestPriorVersionId = "f6e5d4c3b2a1909876543210987654fe";

    /// <summary>
    /// Standard test source request ID.
    /// </summary>
    public const string TestSourceRequestId = "req-12345-abcde";

    /// <summary>
    /// Standard test environment name.
    /// </summary>
    public const string TestEnvironment = "test";

    /// <summary>
    /// Production environment name for environment-specific tests.
    /// </summary>
    public const string ProductionEnvironment = "prod";
}
