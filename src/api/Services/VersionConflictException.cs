namespace SimpleTodo.Api.Services;

/// <summary>
/// Exception thrown when a version conflict is detected during commit operations.
/// Indicates that the current version has changed since the user last loaded it.
/// </summary>
public class VersionConflictException : Exception
{
    /// <summary>
    /// The version ID that was expected to be current.
    /// </summary>
    public string? ExpectedVersionId { get; }

    /// <summary>
    /// The actual current version ID at the time of the conflict.
    /// </summary>
    public string? CurrentVersionId { get; }

    public VersionConflictException(string message) : base(message)
    {
    }

    public VersionConflictException(string message, string? expectedVersionId, string? currentVersionId) 
        : base(message)
    {
        ExpectedVersionId = expectedVersionId;
        CurrentVersionId = currentVersionId;
    }

    public VersionConflictException(string message, Exception innerException) 
        : base(message, innerException)
    {
    }
}
