namespace SimpleTodo.Api.Repositories;

public interface IBlobArtifactRepository
{
    Task SaveBlobAsync(string blobPath, string content);
    Task<string?> GetBlobAsync(string blobPath);
    Task<bool> BlobExistsAsync(string blobPath);
    Task DeleteBlobAsync(string blobPath);
    Task<List<string>> ListBlobsAsync(string prefix);
}
