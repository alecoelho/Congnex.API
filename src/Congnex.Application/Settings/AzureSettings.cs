namespace Congnex.Application.Settings;

public sealed class AzureSettings
{
    public AIFoundrySettings AIFoundry { get; init; } = new();
    public BlobStorageSettings BlobStorage { get; init; } = new();
    public NotificationHubsSettings NotificationHubs { get; init; } = new();
    public CommunicationServicesSettings CommunicationServices { get; init; } = new();
    public BraveSearchSettings BraveSearch { get; init; } = new();
    public YouTubeSettings YouTube { get; init; } = new();
}

public sealed class AIFoundrySettings
{
    public string Endpoint { get; init; } = string.Empty;
    public string ApiKey { get; init; } = string.Empty;
    public string DeploymentName { get; init; } = "gpt-4o";
    public string ApiVersion { get; init; } = "2025-01-01-preview";
    public string EmbeddingDeploymentName { get; init; } = "text-embedding-3-small";
}

public sealed class BlobStorageSettings
{
    public string ConnectionString { get; init; } = string.Empty;
    public string ImagesContainer { get; init; } = "images";
    public string AudioContainer { get; init; } = "audio";
    public string VideoContainer { get; init; } = "video";
    public int SasTokenExpiryMinutes { get; init; } = 60;
}

public sealed class NotificationHubsSettings
{
    public string ConnectionString { get; init; } = string.Empty;
    public string HubName { get; init; } = string.Empty;
}

public sealed class CommunicationServicesSettings
{
    public string ConnectionString { get; init; } = string.Empty;
    public string SenderEmail { get; init; } = string.Empty;
    public string SenderName { get; init; } = "Congnex";
    public string SenderSmsFrom { get; init; } = string.Empty;
}

public sealed class BraveSearchSettings
{
    public string ApiKey { get; init; } = string.Empty;
}

public sealed class YouTubeSettings
{
    public string ApiKey { get; init; } = string.Empty;
}
