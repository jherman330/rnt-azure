using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Text;

namespace SimpleTodo.Api.Repositories;

public class BlobArtifactRepository : IBlobArtifactRepository
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly string _containerName;

    public BlobArtifactRepository(BlobServiceClient blobServiceClient, IConfiguration configuration)
    {
        _blobServiceClient = blobServiceClient;
        _containerName = configuration["AZURE_BLOB_STORAGE_CONTAINER_NAME"] ?? "narrative-artifacts";
    }

    public async Task SaveBlobAsync(string blobPath, string content)
    {
        var containerClient = await GetContainerClientAsync();
        var blobClient = containerClient.GetBlobClient(blobPath);

        var contentBytes = Encoding.UTF8.GetBytes(content);
        await blobClient.UploadAsync(new BinaryData(contentBytes), overwrite: true);
    }

    public async Task<string?> GetBlobAsync(string blobPath)
    {
        var containerClient = await GetContainerClientAsync();
        var blobClient = containerClient.GetBlobClient(blobPath);

        if (!await blobClient.ExistsAsync())
        {
            return null;
        }

        var response = await blobClient.DownloadContentAsync();
        return response.Value.Content.ToString();
    }

    public async Task<bool> BlobExistsAsync(string blobPath)
    {
        var containerClient = await GetContainerClientAsync();
        var blobClient = containerClient.GetBlobClient(blobPath);
        return await blobClient.ExistsAsync();
    }

    public async Task DeleteBlobAsync(string blobPath)
    {
        var containerClient = await GetContainerClientAsync();
        var blobClient = containerClient.GetBlobClient(blobPath);

        if (await blobClient.ExistsAsync())
        {
            await blobClient.DeleteAsync();
        }
    }

    public async Task<List<string>> ListBlobsAsync(string prefix)
    {
        var containerClient = await GetContainerClientAsync();
        var blobNames = new List<string>();

        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix))
        {
            blobNames.Add(blobItem.Name);
        }

        return blobNames;
    }

    private async Task<BlobContainerClient> GetContainerClientAsync()
    {
        var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
        await containerClient.CreateIfNotExistsAsync();
        return containerClient;
    }
}
