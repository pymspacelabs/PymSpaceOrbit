using PymSpaceOrbit.Web.Components;
using PymSpaceOrbit.Shared.Services;
using PymSpaceOrbit.Web.Services;
using PymSpaceOrbit.Shared.Abstractions.Game;
using PymSpaceOrbit.Shared.Abstractions.ML;
using PymSpaceOrbit.Shared.Concrete.Game;
using PymSpaceOrbit.Shared.Concrete.ML;
using PymSpaceOrbit.Shared.Extensions;
using PymSpaceOrbit.Web;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("https://pymspacelabs.github.io/Simulator/")
});
builder.Services.AddPymSpaceOrbitGameServices();
// Add device-specific services used by the PymSpaceOrbit.Shared project
builder.Services.AddSingleton<IFormFactor, FormFactor>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(PymSpaceOrbit.Shared._Imports).Assembly);

app.Run();
