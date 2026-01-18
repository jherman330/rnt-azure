using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Options;
using SimpleTodo.Api;
using SimpleTodo.Api.Middleware;
using SimpleTodo.Api.Models;
using SimpleTodo.Api.Repositories;
using SimpleTodo.Api.Services;
using SimpleTodo.Api.Validation;

var credential = new DefaultAzureCredential();
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<ListsRepository>();
builder.Services.AddSingleton(_ => new CosmosClient(builder.Configuration["AZURE_COSMOS_ENDPOINT"], credential, new CosmosClientOptions()
{
    SerializerOptions = new CosmosSerializationOptions
    {
        PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
    }
}));

// Register HttpClient factory for LLM service
builder.Services.AddHttpClient();

// Configure LLM service settings
builder.Services.Configure<LlmServiceConfiguration>(
    builder.Configuration.GetSection(LlmServiceConstants.ConfigurationSection));

// Register narrative artifact services
builder.Services.AddSingleton<IUserContextService, UserContextService>();

// Register LLM service based on environment
var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
if (environment == "Development" || environment == "Testing")
{
    builder.Services.AddSingleton<ILlmService, MockLlmService>();
}
else
{
    builder.Services.AddSingleton<ILlmService>(sp =>
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var config = sp.GetRequiredService<IOptions<LlmServiceConfiguration>>().Value;
        var logger = sp.GetRequiredService<ILogger<OpenAILlmService>>();
        return new OpenAILlmService(httpClientFactory, Options.Create(config), logger);
    });
}

builder.Services.AddSingleton<IPromptTemplateService, PromptTemplateService>();

// Register Prompt Factory
builder.Services.AddPromptFactory(builder.Configuration);

builder.Services.AddSingleton<StoryRootValidator>();
builder.Services.AddSingleton<WorldStateValidator>();
builder.Services.AddSingleton<IStoryRootService, StoryRootService>();
builder.Services.AddSingleton<IWorldStateService, WorldStateService>();

// Blob Storage client registration (optional - only when endpoint is provided)
var blobStorageEndpoint = builder.Configuration["AZURE_BLOB_STORAGE_ENDPOINT"];
if (!string.IsNullOrEmpty(blobStorageEndpoint))
{
    builder.Services.AddSingleton(_ => new BlobServiceClient(new Uri(blobStorageEndpoint), credential));
    builder.Services.AddSingleton<IBlobArtifactRepository, BlobArtifactRepository>();
    builder.Services.AddSingleton<IStoryRootRepository, StoryRootRepository>();
    builder.Services.AddSingleton<IWorldStateRepository, WorldStateRepository>();
}

builder.Services.AddCors();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Correlation ID middleware must be first in the pipeline
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseCors(policy =>
{
    policy.AllowAnyOrigin();
    policy.AllowAnyHeader();
    policy.AllowAnyMethod();
});

// Swagger UI
app.UseSwaggerUI(options => {
    options.SwaggerEndpoint("./openapi.yaml", "v1");
    options.RoutePrefix = "";
});

app.UseStaticFiles(new StaticFileOptions{
    // Serve openapi.yaml file
    ServeUnknownFileTypes = true,
});

app.MapGroup("/lists")
    .MapTodoApi()
    .WithOpenApi();

app.MapGroup("/api/story-root")
    .MapStoryRootApi()
    .WithOpenApi();

app.MapGroup("/api/world-state")
    .MapWorldStateApi()
    .WithOpenApi();

app.Run();

// Make Program visible to test assembly for WebApplicationFactory
public partial class Program { }