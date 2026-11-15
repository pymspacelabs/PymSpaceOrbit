using Microsoft.Extensions.Logging;
using PymSpaceOrbit.Shared.Services;
using PymSpaceOrbit.Services;
using PymSpaceOrbit.Shared.Abstractions.Game;
using PymSpaceOrbit.Shared.Abstractions.ML;
using PymSpaceOrbit.Shared.Concrete.Game;
using PymSpaceOrbit.Shared.Concrete.ML;
using PymSpaceOrbit.Shared.Extensions;
using MudBlazor.Services;
using Plugin.Maui.Audio;
namespace PymSpaceOrbit;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .AddAudio(playbackOptions =>
            {
#if IOS || MACCATALYST
                playbackOptions.Category = AVFoundation.AVAudioSessionCategory.Playback;
                playbackOptions.Mode = AVFoundation.AVAudioSessionMode.Default;
                playbackOptions.CategoryOptions = AVFoundation.AVAudioSessionCategoryOptions.DefaultToSpeaker;
#elif ANDROID
                playbackOptions.AudioContentType = Android.Media.AudioContentType.Music;
                playbackOptions.AudioUsageKind = Android.Media.AudioUsageKind.Media;
#endif
            })
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
            });

        // Add device-specific services used by the PymSpaceOrbit.Shared project
        builder.Services.AddSingleton<IFormFactor, FormFactor>();
        builder.Services.AddSingleton<IAudioService, AudioService>();

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
        builder.Services.AddSingleton<App>();
        builder.Services.AddSingleton<MainPage>();
        return builder.Build();
    }
}
