using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using SimpleTodo.Api;
using SimpleTodo.Api.Middleware;
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

// Register narrative artifact services
builder.Services.AddSingleton<IUserContextService, UserContextService>();
builder.Services.AddSingleton<ILlmService, PlaceholderLlmService>();
builder.Services.AddSingleton<IPromptTemplateService, PromptTemplateService>();
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