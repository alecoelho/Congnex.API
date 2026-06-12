namespace Congnex.Application.Interfaces;

public interface IBlobStorageService
{
    Task<string> GetSasUrlAsync(string container, string blobPath);
    Task<string> UploadAsync(string container, string blobPath, Stream content, string contentType);
    Task DeleteAsync(string container, string blobPath);
}
