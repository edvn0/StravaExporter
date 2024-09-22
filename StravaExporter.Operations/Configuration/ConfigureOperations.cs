namespace StravaExporter.Operations.Configuration;

using MediatR;
using Microsoft.Extensions.DependencyInjection;

public static class ConfigureOperations
{
    public static void ConfigureOperationsEndpoints(IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<AssemblyMarker>());
    }
}