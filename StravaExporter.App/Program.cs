using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http;
using StravaExporter.Models;
using StravaExporter.Services;
using StravaExporter.Configuration;

// Build configuration
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

// Set up DI container
var services = new ServiceCollection();

// Register configuration and services
services.Configure<StravaOptions>(configuration.GetSection("Strava"));
services.AddHttpClient<StravaService>((provider, opts) =>
{
    var stravaOptions = provider.GetRequiredService<IOptions<StravaOptions>>().Value;
    opts.BaseAddress = new Uri(stravaOptions.BaseUrl);
    opts.DefaultRequestHeaders.Add("Accept", "application/json");
    opts.DefaultRequestHeaders.Add("Bearer", configuration.GetSection("Strava")["AccessToken"]!);
});
services.AddTransient<IStravaService, StravaService>((provider) =>
{
    var clientFactory = provider.GetRequiredService<IHttpClientFactory>();
    var stravaOptions = provider.GetRequiredService<IOptions<StravaOptions>>().Value;
    return new StravaService(clientFactory, stravaOptions);
});

// Build service provider
var serviceProvider = services.BuildServiceProvider();

// Resolve StravaService and use it
var stravaService = serviceProvider.GetRequiredService<IStravaService>();

// Make a request to Strava API
var activities = await stravaService.GetActivitiesAsync();
foreach (var activity in activities)
{
    Console.WriteLine(activity.Name);
}
