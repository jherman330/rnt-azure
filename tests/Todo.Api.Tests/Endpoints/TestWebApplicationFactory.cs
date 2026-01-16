using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using SimpleTodo.Api.Repositories;
using SimpleTodo.Api.Services;
using SimpleTodo.Api.Validation;
using Todo.Api.Tests.Mocks;
using Todo.Api.Tests.TestUtilities;

namespace Todo.Api.Tests.Endpoints;

/// <summary>
/// WebApplicationFactory for integration testing of API endpoints.
/// Configures the test server with mocked dependencies.
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<SimpleTodo.Api.ProgramMarker>
{
    private readonly Mock<IStoryRootRepository> _storyRootRepositoryMock;
    private readonly Mock<IWorldStateRepository> _worldStateRepositoryMock;
    private readonly MockLlmService _llmServiceMock;
    private readonly Mock<IPromptTemplateService> _promptTemplateServiceMock;
    private readonly Mock<IUserContextService> _userContextServiceMock;

    public TestWebApplicationFactory()
    {
        _storyRootRepositoryMock = new Mock<IStoryRootRepository>();
        _worldStateRepositoryMock = new Mock<IWorldStateRepository>();
        _llmServiceMock = new MockLlmService();
        _promptTemplateServiceMock = new Mock<IPromptTemplateService>();
        _userContextServiceMock = TestUtilities.MockExtensions.CreateUserContextServiceMock();

        // Setup default prompt template
        _promptTemplateServiceMock
            .Setup(s => s.GetPromptTemplateAsync(It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("Template with {current_story_root} and {user_input}");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove existing service registrations
            var storyRootServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IStoryRootService));
            if (storyRootServiceDescriptor != null)
            {
                services.Remove(storyRootServiceDescriptor);
            }

            var worldStateServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IWorldStateService));
            if (worldStateServiceDescriptor != null)
            {
                services.Remove(worldStateServiceDescriptor);
            }

            var llmServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(ILlmService));
            if (llmServiceDescriptor != null)
            {
                services.Remove(llmServiceDescriptor);
            }

            var promptTemplateServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IPromptTemplateService));
            if (promptTemplateServiceDescriptor != null)
            {
                services.Remove(promptTemplateServiceDescriptor);
            }

            var userContextServiceDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IUserContextService));
            if (userContextServiceDescriptor != null)
            {
                services.Remove(userContextServiceDescriptor);
            }

            var storyRootValidatorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(StoryRootValidator));
            if (storyRootValidatorDescriptor != null)
            {
                services.Remove(storyRootValidatorDescriptor);
            }

            var worldStateValidatorDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(WorldStateValidator));
            if (worldStateValidatorDescriptor != null)
            {
                services.Remove(worldStateValidatorDescriptor);
            }

            // Register mocked services
            services.AddSingleton(_storyRootRepositoryMock.Object);
            services.AddSingleton(_worldStateRepositoryMock.Object);
            services.AddSingleton<ILlmService>(_llmServiceMock);
            services.AddSingleton(_promptTemplateServiceMock.Object);
            services.AddSingleton(_userContextServiceMock.Object);
            services.AddSingleton<StoryRootValidator>();
            services.AddSingleton<WorldStateValidator>();
            services.AddSingleton<IStoryRootService, StoryRootService>();
            services.AddSingleton<IWorldStateService, WorldStateService>();

            // Disable blob storage registration for tests
            // This prevents errors when blob storage endpoint is not configured
        });
    }

    public Mock<IStoryRootRepository> StoryRootRepositoryMock => _storyRootRepositoryMock;
    public Mock<IWorldStateRepository> WorldStateRepositoryMock => _worldStateRepositoryMock;
    public MockLlmService LlmServiceMock => _llmServiceMock;
    public Mock<IPromptTemplateService> PromptTemplateServiceMock => _promptTemplateServiceMock;
    public Mock<IUserContextService> UserContextServiceMock => _userContextServiceMock;
}
