using PymSpaceOrbit.Shared.Abstractions.Localization;
using System.Resources;
using Microsoft.Extensions.Localization;
namespace PymSpaceOrbit.Shared.Concrete.Localization;

public class Localizer<T>(IStringLocalizerFactory factory, LocalizationState localizationState) : ILocalizer<T>
{
    private readonly IStringLocalizer _localizer =
        factory.Create("Strings", typeof(T).Assembly.FullName!);
    private readonly ResourceManager _resourceManager = new(
        $"{typeof(T).Assembly.GetName().Name}.Resources.Localization.Strings",
        typeof(T).Assembly);

    public string this[string key]
    {
        get
        {
            // Resolve the resource using the selected culture explicitly. This
            // avoids relying on the renderer's execution-context culture, which
            // may still carry the startup culture after a runtime switch.
            return _resourceManager.GetString(key, localizationState.CurrentCulture)
                ?? _localizer[key];
        }
    }
}
