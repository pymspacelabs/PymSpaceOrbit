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
    }

    public void SetCulture(string cultureName)
    {
        var culture = CultureInfo.GetCultureInfo(cultureName);
        if (Equals(CurrentCulture, culture)) return;
        CurrentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureChanged?.Invoke();
    }
}
