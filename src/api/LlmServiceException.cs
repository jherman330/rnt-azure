namespace SimpleTodo.Api.Services;

/// <summary>
/// Base exception for LLM service errors.
/// </summary>
public class LlmServiceException : Exception
{
    /// <summary>
    /// Correlation ID associated with the request that caused this exception.
    /// </summary>
    public string? CorrelationId { get; }

    public LlmServiceException(string message) : base(message)
    {
    }

    public LlmServiceException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public LlmServiceException(string message, string? correlationId) : base(message)
    {
        CorrelationId = correlationId;
    }

    public LlmServiceException(string message, Exception innerException, string? correlationId) : base(message, innerException)
    {
        CorrelationId = correlationId;
    }
}

/// <summary>
/// Exception thrown when a network or HTTP error occurs during LLM service calls.
/// </summary>
public class LlmNetworkException : LlmServiceException
{
    public LlmNetworkException(string message) : base(message)
    {
    }

    public LlmNetworkException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public LlmNetworkException(string message, string? correlationId) : base(message, correlationId)
    {
    }

    public LlmNetworkException(string message, Exception innerException, string? correlationId) : base(message, innerException, correlationId)
    {
    }
}

/// <summary>
/// Exception thrown when authentication fails during LLM service calls (e.g., invalid API key).
/// </summary>
public class LlmAuthenticationException : LlmServiceException
{
    public LlmAuthenticationException(string message) : base(message)
    {
    }

    public LlmAuthenticationException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public LlmAuthenticationException(string message, string? correlationId) : base(message, correlationId)
    {
    }

    public LlmAuthenticationException(string message, Exception innerException, string? correlationId) : base(message, innerException, correlationId)
    {
    }
}

/// <summary>
/// Exception thrown when rate limits are exceeded during LLM service calls.
/// </summary>
public class LlmRateLimitException : LlmServiceException
{
    public LlmRateLimitException(string message) : base(message)
    {
    }

    public LlmRateLimitException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public LlmRateLimitException(string message, string? correlationId) : base(message, correlationId)
    {
    }

    public LlmRateLimitException(string message, Exception innerException, string? correlationId) : base(message, innerException, correlationId)
    {
    }
}
