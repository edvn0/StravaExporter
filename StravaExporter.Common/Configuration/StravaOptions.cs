namespace StravaExporter.Configuration;

public class StravaOptions
{
    public required string ClientId { get; init; }
    public required string ClientSecret { get; init; }
    public required string AccessToken { get; init; }
    public required string BaseUrl { get; init; }
    public required string RefreshToken { get; init; }
}
