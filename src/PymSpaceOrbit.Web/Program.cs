using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using PymSpaceOrbit.Shared.Abstractions.Game;
using PymSpaceOrbit.Shared.Extensions;
using PymSpaceOrbit.Shared.Services;
using PymSpaceOrbit.Web;
using PymSpaceOrbit.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://pymspacelabs.github.io/Simulator/")
});

builder.Services.AddPymSpaceOrbitGameServices();
builder.Services.AddMudServices();

// Add device-specific services used by the PymSpaceOrbit.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();
builder.Services.AddScoped<IAudioService, AudioService>();

var app  = builder.Build();


await app.RunAsync();
 