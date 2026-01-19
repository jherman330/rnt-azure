using Microsoft.Extensions.DependencyInjection;

namespace SimpleTodo.Api.Services;

/// <summary>
/// Extension methods for registering Story Root prompt builder services with dependency injection.
/// </summary>
public static class StoryRootPromptBuilderExtensions
{
    /// <summary>
    /// Registers Story Root prompt builder and its dependencies with the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddStoryRootPromptBuilder(this IServiceCollection services)
    {
        // Register the prompt builder service
        services.AddSingleton<IStoryRootPromptBuilder, StoryRootPromptBuilder>();

        return services;
    }
}
