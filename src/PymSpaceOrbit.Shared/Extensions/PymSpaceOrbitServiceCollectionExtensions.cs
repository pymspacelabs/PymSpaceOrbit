using Microsoft.Extensions.DependencyInjection;
using PymSpaceOrbit.Shared.Abstractions.Game;
using PymSpaceOrbit.Shared.Abstractions.Localization;
using PymSpaceOrbit.Shared.Abstractions.ML;
using PymSpaceOrbit.Shared.Concrete.Game;
using PymSpaceOrbit.Shared.Concrete.Localization;
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
        services.AddScoped<PymSpaceOrbitMlModelService>();
        services.AddScoped<IPymSpaceOrbitMlModelService>(sp => sp.GetRequiredService<PymSpaceOrbitMlModelService>());
        services.AddScoped<IPymGameOrbitMlModelService>(sp => sp.GetRequiredService<PymSpaceOrbitMlModelService>());
        services.AddScoped<IBioShipMlModelService>(sp => sp.GetRequiredService<PymSpaceOrbitMlModelService>());
        services.AddScoped<IGameService, GameService>();
        services.AddScoped<PymSpaceOrbit.Shared.Services.NasaCardImageResolver>();
        services.AddLocalization(options => options.ResourcesPath = "Resources/Localization");
        services.AddSingleton(typeof(ILocalizer<>), typeof(Localizer<>));
        services.AddScoped<LocalizationState>();
        return services;
    }
}
