using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SimpleTodo.Api.Services;

/// <summary>
/// Extension methods for registering Prompt Factory services with dependency injection.
/// </summary>
public static class PromptFactoryExtensions
{
    /// <summary>
    /// Configuration section name for Prompt Factory settings.
    /// </summary>
    public const string ConfigurationSection = "PromptFactory";

    /// <summary>
    /// Registers Prompt Factory and its dependencies with the service collection.
    /// </summary>
    /// <param name="services">The service collection to add services to</param>
    /// <param name="configuration">Optional configuration to bind PromptFactoryOptions from</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddPromptFactory(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        // Register configuration if provided
        if (configuration != null)
        {
            services.Configure<PromptFactoryOptions>(
                configuration.GetSection(ConfigurationSection));
        }

        // Register template loader
        services.AddSingleton<ITemplateLoader, EmbeddedTemplateLoader>();

        // Register variable substitutor
        services.AddSingleton<IVariableSubstitutor, SimpleVariableSubstitutor>();

        // Register prompt factory
        services.AddSingleton<IPromptFactory, PromptFactory>();

        return services;
    }
}
