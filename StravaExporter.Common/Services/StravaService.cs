using System.Net.Http.Json;
using StravaExporter.Models;
using StravaExporter.Configuration;
using System.Text.Json;
using System.Net.Http.Headers;

namespace StravaExporter.Services;

public interface IStravaService
{
  public Task<IEnumerable<StravaActivity>> GetActivitiesAsync(CancellationToken cancellationToken = default);
  /// <summary>
  /// Get a new access token using the refresh token
  /// </summary>
  /// <param name="cancellationToken"></param>
  /// <throws>HttpRequestException</throws>
  /// <returns></returns>
  public Task<StravaRefreshToken> GetRefreshTokenAsync(CancellationToken cancellationToken = default);
}

public class StravaService(IHttpClientFactory factory, StravaOptions options) : IStravaService
{

  private static JsonSerializerOptions s_serializerOptions = new()
  {
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
  };

  public async Task<IEnumerable<StravaActivity>> GetActivitiesAsync(CancellationToken cancellationToken = default)
  {
    var client = factory.CreateClient("StravaService");

    StravaRefreshToken? refreshAccessToken;
    try
    {
      refreshAccessToken = await GetRefreshTokenAsync(cancellationToken);
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
      return [];
    }

    client.DefaultRequestHeaders.Remove("Bearer");
    // Add Authorization Bearer token
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", refreshAccessToken?.AccessToken);
    var response = await client.GetAsync("api/v3/athlete/activities", cancellationToken);
    if (!response.IsSuccessStatusCode)
    {
      return [];
    }

    var stravaActivities = await response.Content.ReadAsStreamAsync(cancellationToken);

    try
    {
      var debug = await response.Content.ReadAsStringAsync(cancellationToken);

      var deserialised = await JsonSerializer.DeserializeAsync<IEnumerable<StravaActivity>>(stravaActivities, s_serializerOptions, cancellationToken);

      return deserialised ?? [];
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
      return [];
    }

  }

  public async Task<StravaRefreshToken> GetRefreshTokenAsync(CancellationToken cancellationToken = default)
  {
    var client = factory.CreateClient("StravaService");
    var response = await client.PostAsync("api/v3/oauth/token", JsonContent.Create(new
    {
      client_id = options.ClientId,
      client_secret = options.ClientSecret,
      refresh_token = options.RefreshToken,
      grant_type = "refresh_token"
    }), cancellationToken);

    try
    {
      if (!response.IsSuccessStatusCode)
      {
        throw new HttpRequestException($"Failed to get refresh token: {response.ReasonPhrase}");
      }
      var stravaRefreshTokenStream = await response.Content.ReadAsStreamAsync(cancellationToken);

      // Debug to string
      var stravaRefreshTokenString = await response.Content.ReadAsStringAsync(cancellationToken);

      var stravaRefreshToken = await JsonSerializer.DeserializeAsync<StravaRefreshToken>(stravaRefreshTokenStream, s_serializerOptions, cancellationToken);

      return stravaRefreshToken;
    }
    catch (Exception e)
    {
      Console.WriteLine(e.Message);
      throw;
    }
  }

}
