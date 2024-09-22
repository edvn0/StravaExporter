namespace StravaExporter.Models;

public readonly struct StravaRefreshToken
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresAt { get; init; } // Seconds since epoch
    public required int ExpiresIn { get; init; } // Seconds until expiration
}