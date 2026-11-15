using Microsoft.Extensions.DependencyInjection;
using PymSpaceOrbit.Shared.Abstractions.Game;
using PymSpaceOrbit.Shared.Abstractions.ML;
using PymSpaceOrbit.Shared.Concrete.Game;
using PymSpaceOrbit.Shared.Concrete.ML;

namespace PymSpaceOrbit.Shared.Extensions;

public static class PymSpaceOrbitServiceCollectionExtensions
{
    /// <summary>
    /// Registers IPymSpaceOrbitMlModelService -> PymSpaceOrbitMlModelService and IGameService -> GameService
    /// for both PymSpaceOrbit (.NET MAUI Blazor Hybrid) and PymSpaceOrbit.Web (Blazor WebAssembly).
    /// </summary>
    public static IServiceCollection AddPymSpaceOrbitGameServices(this IServiceCollection services)
    {
        services.AddScoped<IPymSpaceOrbitMlModelService, PymSpaceOrbitMlModelService>();
        services.AddScoped<IGameService, GameService>();
        return services;
    }
}
