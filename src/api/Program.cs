using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using SimpleTodo.Api;
using SimpleTodo.Api.Repositories;
using SimpleTodo.Api.Services;

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

// Blob Storage client registration
var blobStorageEndpoint = builder.Configuration["AZURE_BLOB_STORAGE_ENDPOINT"];
if (!string.IsNullOrEmpty(blobStorageEndpoint))
{
    builder.Services.AddSingleton(_ => new BlobServiceClient(new Uri(blobStorageEndpoint), credential));
}
else
{
    throw new InvalidOperationException("AZURE_BLOB_STORAGE_ENDPOINT configuration is required");
}

// Register narrative artifact services
builder.Services.AddSingleton<IUserContextService, UserContextService>();
builder.Services.AddSingleton<IBlobArtifactRepository, BlobArtifactRepository>();
builder.Services.AddSingleton<IStoryRootRepository, StoryRootRepository>();
builder.Services.AddSingleton<IWorldStateRepository, WorldStateRepository>();

builder.Services.AddCors();
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

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
app.Run();