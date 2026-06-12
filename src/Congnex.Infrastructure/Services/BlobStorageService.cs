using Azure.Storage.Blobs;
using Azure.Storage.Sas;
using Congnex.Application.Interfaces;
using Congnex.Application.Settings;
using Microsoft.Extensions.Options;

namespace Congnex.Infrastructure.Services;

public sealed class BlobStorageService(IOptions<AzureSettings> opts) : IBlobStorageService
{
    private readonly BlobStorageSettings _cfg = opts.Value.BlobStorage;

    private BlobContainerClient Container(string container) =>
        new BlobContainerClient(_cfg.ConnectionString, container);

    public async Task<string> GetSasUrlAsync(string container, string blobPath)
    {
        var client = Container(container).GetBlobClient(blobPath);
        var sasUri = client.GenerateSasUri(BlobSasPermissions.Read,
            DateTimeOffset.UtcNow.AddMinutes(_cfg.SasTokenExpiryMinutes));
        return await Task.FromResult(sasUri.ToString());
    }

    public async Task<string> UploadAsync(string container, string blobPath, Stream content, string contentType)
    {
        var client = Container(container).GetBlobClient(blobPath);
        await client.UploadAsync(content, overwrite: true);
        return blobPath;
    }

    public async Task DeleteAsync(string container, string blobPath)
    {
        var client = Container(container).GetBlobClient(blobPath);
        await client.DeleteIfExistsAsync();
    }
}
