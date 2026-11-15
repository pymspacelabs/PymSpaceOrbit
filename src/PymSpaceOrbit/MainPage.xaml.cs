using Microsoft.AspNetCore.Components.WebView;

namespace PymSpaceOrbit;

public partial class MainPage : ContentPage
{
    public MainPage()
    {
        InitializeComponent();
    }

    private void OnBlazorWebViewInitializing(object? sender, BlazorWebViewInitializingEventArgs e)
    {
#if WINDOWS
        e.EnvironmentOptions ??= new Microsoft.Web.WebView2.Core.CoreWebView2EnvironmentOptions();
        e.EnvironmentOptions.AdditionalBrowserArguments = "--autoplay-policy=no-user-gesture-required";
#elif IOS || MACCATALYST
        if (e.Configuration is not null)
        {
            e.Configuration.AllowsInlineMediaPlayback = true;
            e.Configuration.MediaTypesRequiringUserActionForPlayback = WebKit.WKAudiovisualMediaTypes.None;
        }
#endif
    }

    private void OnBlazorWebViewInitialized(object? sender, BlazorWebViewInitializedEventArgs e)
    {
#if ANDROID
        if (e.WebView?.Settings is not null)
        {
            e.WebView.Settings.MediaPlaybackRequiresUserGesture = false;
            e.WebView.Settings.JavaScriptEnabled = true;
            e.WebView.Settings.DomStorageEnabled = true;
        }
#endif
    }
}
