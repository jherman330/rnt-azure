namespace SimpleTodo.Api.Services.StoryRoot;

/// <summary>
/// Exception thrown when Story Root prompt builder encounters invalid or missing context.
/// </summary>
public class StoryRootPromptBuilderException : Exception
{
    public StoryRootPromptBuilderException(string message) : base(message)
    {
    }

    public StoryRootPromptBuilderException(string message, Exception innerException) : base(message, innerException)
    {
    }
}

/// <summary>
/// Exception thrown when Story Root prompt builder encounters an unsupported operation.
/// </summary>
public class UnsupportedStoryRootOperationException : StoryRootPromptBuilderException
{
    public UnsupportedStoryRootOperationException(string operation) 
        : base($"Unsupported Story Root operation: {operation}")
    {
        Operation = operation;
    }

    public string Operation { get; }
}

/// <summary>
/// Exception thrown when Story Root prompt builder encounters invalid user input.
/// </summary>
public class InvalidUserInputException : StoryRootPromptBuilderException
{
    public InvalidUserInputException(string message) : base(message)
    {
    }

    public InvalidUserInputException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
