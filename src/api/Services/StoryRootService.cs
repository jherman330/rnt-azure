using SimpleTodo.Api.Models;
using SimpleTodo.Api.Repositories;
using SimpleTodo.Api.Validation;
using System.Text.Json;

namespace SimpleTodo.Api.Services;

/// <summary>
/// Service layer implementation for Story Root operations.
/// Orchestrates calls to repositories, LLM services, and validators,
/// handling business logic, parsing, validation, and merge operations.
/// </summary>
public class StoryRootService : IStoryRootService
{
    private readonly IStoryRootRepository _repository;
    private readonly ILlmService _llmService;
    private readonly IStoryRootPromptBuilder _storyRootPromptBuilder;
    private readonly IPromptFactory _promptFactory;
    private readonly IUserContextService _userContextService;
    private readonly StoryRootValidator _validator;
    private readonly JsonSerializerOptions _jsonOptions;

    public StoryRootService(
        IStoryRootRepository repository,
        ILlmService llmService,
        IStoryRootPromptBuilder storyRootPromptBuilder,
        IPromptFactory promptFactory,
        IUserContextService userContextService,
        StoryRootValidator validator)
    {
        _repository = repository;
        _llmService = llmService;
        _storyRootPromptBuilder = storyRootPromptBuilder;
        _promptFactory = promptFactory;
        _userContextService = userContextService;
        _validator = validator;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<Models.StoryRoot?> GetCurrentStoryRootAsync()
    {
        var userId = _userContextService.GetCurrentUserId();
        return await _repository.GetCurrentVersionAsync(userId);
    }

    public async Task<Models.StoryRoot?> GetStoryRootVersionAsync(string versionId)
    {
        if (string.IsNullOrWhiteSpace(versionId))
        {
            throw new ArgumentException("Version ID cannot be null or empty", nameof(versionId));
        }

        var userId = _userContextService.GetCurrentUserId();
        return await _repository.GetVersionAsync(userId, versionId);
    }

    public async Task<List<VersionMetadata>> ListStoryRootVersionsAsync()
    {
        var userId = _userContextService.GetCurrentUserId();
        return await _repository.ListVersionsAsync(userId);
    }

    public async Task<Models.StoryRoot> ProposeStoryRootMergeAsync(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
        {
            throw new ArgumentException("Raw input cannot be null or empty", nameof(rawInput));
        }

        // Get current Story Root (if exists)
        var currentStoryRoot = await GetCurrentStoryRootAsync();

        // Prepare prompt inputs using domain logic
        var promptInput = await _storyRootPromptBuilder.PrepareStoryRootMergeAsync(currentStoryRoot, rawInput);

        // Assemble prompt string using Prompt Factory
        var prompt = await _promptFactory.AssemblePromptAsync(promptInput.TemplateId, promptInput.Variables);

        // Call LLM service
        string llmResponse;
        try
        {
            llmResponse = await _llmService.InvokeAsync(prompt);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"LLM service call failed: {ex.Message}", ex);
        }

        // Parse LLM response
        if (string.IsNullOrWhiteSpace(llmResponse))
        {
            throw new InvalidOperationException("LLM service returned empty response");
        }

        Models.StoryRoot? proposal;
        try
        {
            // Try to parse the JSON response
            proposal = JsonSerializer.Deserialize<Models.StoryRoot>(llmResponse, _jsonOptions);
        }
        catch (JsonException ex)
        {
            throw new InvalidOperationException($"Failed to parse LLM response as JSON: {ex.Message}. Response: {llmResponse}", ex);
        }

        if (proposal == null)
        {
            throw new InvalidOperationException($"LLM response deserialized to null. Response: {llmResponse}");
        }

        // Validate the proposal
        var (isValid, errorMessage) = _validator.Validate(proposal);
        if (!isValid)
        {
            throw new InvalidOperationException($"Proposal validation failed: {errorMessage}. Response: {llmResponse}");
        }

        return proposal;
    }

    public async Task<string> CommitStoryRootVersionAsync(Models.StoryRoot proposal, string? identity = null, string? expectedVersionId = null)
    {
        if (proposal == null)
        {
            throw new ArgumentNullException(nameof(proposal));
        }

        // Validate the proposal before committing
        var (isValid, errorMessage) = _validator.Validate(proposal);
        if (!isValid)
        {
            throw new ArgumentException($"Proposal validation failed: {errorMessage}", nameof(proposal));
        }

        // Get current version to establish prior version link and check for conflicts
        // The repository returns versions ordered by timestamp descending (newest first)
        var versions = await ListStoryRootVersionsAsync();
        var currentVersionId = versions.FirstOrDefault()?.VersionId;

        // Version conflict detection (if expectedVersionId provided)
        if (!string.IsNullOrWhiteSpace(expectedVersionId))
        {
            if (currentVersionId != expectedVersionId)
            {
                throw new VersionConflictException(
                    $"Expected version {expectedVersionId}, but current version is {currentVersionId ?? "(none)"}",
                    expectedVersionId,
                    currentVersionId);
            }
        }

        var priorVersionId = currentVersionId;

        // Generate source request ID for provenance
        var sourceRequestId = identity ?? Guid.NewGuid().ToString("N");

        // Get environment (could be from configuration in future)
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

        // Save new version with llm_assisted flag set to true
        var versionId = await _repository.SaveNewVersionAsync(
            proposal,
            priorVersionId: priorVersionId,
            sourceRequestId: sourceRequestId,
            environment: environment,
            llmAssisted: true);

        return versionId;
    }
}
