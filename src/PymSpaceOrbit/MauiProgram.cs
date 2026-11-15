using Microsoft.Extensions.Logging;
using PymSpaceOrbit.Shared.Services;
using PymSpaceOrbit.Services;
using PymSpaceOrbit.Shared.Abstractions.Game;
using PymSpaceOrbit.Shared.Abstractions.ML;
using PymSpaceOrbit.Shared.Concrete.Game;
using PymSpaceOrbit.Shared.Concrete.ML;
using PymSpaceOrbit.Shared.Extensions;
using MudBlazor.Services;
namespace PymSpaceOrbit;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the PymSpaceOrbit.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();

        builder.Services.AddMauiBlazorWebView();

#if DEBUG
        builder.Services.AddBlazorWebViewDeveloperTools();
        builder.Logging.AddDebug();
#endif
        builder.Services.AddScoped(sp => new HttpClient
        {
            BaseAddress = new Uri("https://pymspacelabs.github.io/Simulator/")
        });

        builder.Services.AddPymSpaceOrbitGameServices();
        builder.Services.AddMudServices();

        return builder.Build();
    }
}
