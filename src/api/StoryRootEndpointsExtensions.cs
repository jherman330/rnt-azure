using Microsoft.AspNetCore.Http.HttpResults;
using SimpleTodo.Api.Middleware;
using SimpleTodo.Api.Models;
using SimpleTodo.Api.Services;

namespace SimpleTodo.Api;

/// <summary>
/// Extension methods for mapping Story Root API endpoints following the minimal APIs pattern.
/// </summary>
public static class StoryRootEndpointsExtensions
{
    public static RouteGroupBuilder MapStoryRootApi(this RouteGroupBuilder group)
    {
        group.MapGet("/", GetCurrentStoryRoot);
        group.MapGet("/versions/{versionId}", GetStoryRootVersion);
        group.MapGet("/versions", GetStoryRootVersions);
        group.MapPost("/propose-merge", ProposeStoryRootMerge);
        group.MapPost("/commit", CommitStoryRoot);
        return group;
    }

    /// <summary>
    /// GET /api/story-root - Returns the current Story Root for the authenticated user.
    /// </summary>
    public static async Task<IResult> GetCurrentStoryRoot(
        IStoryRootService storyRootService,
        HttpContext context)
    {
        try
        {
            var storyRoot = await storyRootService.GetCurrentStoryRootAsync();

            if (storyRoot == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(storyRoot);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"An error occurred while retrieving the current Story Root: {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 500);
        }
    }

    /// <summary>
    /// GET /api/story-root/versions/{versionId} - Returns a specific version of the Story Root.
    /// </summary>
    public static async Task<IResult> GetStoryRootVersion(
        IStoryRootService storyRootService,
        string versionId,
        HttpContext context)
    {
        if (string.IsNullOrWhiteSpace(versionId))
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = "Version ID is required.",
                    CorrelationId = correlationId
                },
                statusCode: 400);
        }

        try
        {
            var storyRoot = await storyRootService.GetStoryRootVersionAsync(versionId);

            if (storyRoot == null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Ok(storyRoot);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"An error occurred while retrieving Story Root version: {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 500);
        }
    }

    /// <summary>
    /// GET /api/story-root/versions - Returns a list of all Story Root versions for the authenticated user.
    /// </summary>
    public static async Task<IResult> GetStoryRootVersions(
        IStoryRootService storyRootService,
        HttpContext context)
    {
        try
        {
            var versions = await storyRootService.ListStoryRootVersionsAsync();
            return TypedResults.Ok(versions);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"An error occurred while retrieving Story Root versions: {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 500);
        }
    }

    /// <summary>
    /// POST /api/story-root/propose-merge - Generates an LLM proposal for merging raw input into the Story Root.
    /// </summary>
    public static async Task<IResult> ProposeStoryRootMerge(
        IStoryRootService storyRootService,
        ProposeStoryRootMergeRequest request,
        HttpContext context)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.RawInput))
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = "Request body is required and must include a non-empty 'raw_input' field.",
                    CorrelationId = correlationId
                },
                statusCode: 400);
        }

        try
        {
            var currentStoryRoot = await storyRootService.GetCurrentStoryRootAsync();
            var proposal = await storyRootService.ProposeStoryRootMergeAsync(request.RawInput);

            var response = new ProposalResponse<StoryRoot>
            {
                Proposal = proposal,
                Current = currentStoryRoot
            };

            return TypedResults.Ok(response);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"An error occurred while proposing Story Root merge: {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 500);
        }
    }

    /// <summary>
    /// POST /api/story-root/commit - Commits a Story Root proposal as a new version.
    /// </summary>
    public static async Task<IResult> CommitStoryRoot(
        IStoryRootService storyRootService,
        CommitStoryRootRequest request,
        HttpContext context)
    {
        if (request == null || request.StoryRoot == null)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = "Request body is required and must include a 'story_root' field.",
                    CorrelationId = correlationId
                },
                statusCode: 400);
        }

        try
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            var versionId = await storyRootService.CommitStoryRootVersionAsync(
                request.StoryRoot, 
                correlationId,
                request.ExpectedVersionId);

            var response = new CommitResponse<StoryRoot>
            {
                VersionId = versionId,
                Artifact = request.StoryRoot
            };

            return TypedResults.Ok(response);
        }
        catch (VersionConflictException ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"Story Root has been updated by another user. {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 409);
        }
        catch (Exception ex)
        {
            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
            return TypedResults.Json(
                new ErrorResponse
                {
                    Error = $"An error occurred while committing Story Root: {ex.Message}",
                    CorrelationId = correlationId
                },
                statusCode: 500);
        }
    }
}
