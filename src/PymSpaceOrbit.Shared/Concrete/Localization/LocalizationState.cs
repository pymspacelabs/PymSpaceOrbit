using System.Globalization;

namespace PymSpaceOrbit.Shared.Concrete.Localization;


public sealed class LocalizationState
{
    public event Action? CultureChanged;
    public CultureInfo CurrentCulture { get; private set; } = CultureInfo.GetCultureInfo("en-US");

    public LocalizationState()
    {
        CultureInfo.CurrentCulture = CurrentCulture;
        CultureInfo.CurrentUICulture = CurrentCulture;
        CultureInfo.DefaultThreadCurrentCulture = CurrentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CurrentCulture;
    }

    public void SetCulture(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);
        if (Equals(CurrentCulture, culture)) return;
        CurrentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureChanged?.Invoke();
    }
}
