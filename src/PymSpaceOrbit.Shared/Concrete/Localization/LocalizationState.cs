using System.Globalization;

namespace PymSpaceOrbit.Shared.Concrete.Localization;


public sealed class LocalizationState
{
    public event Action? CultureChanged;
    public CultureInfo CurrentCulture { get; private set; }

    public LocalizationState()
    {
        CurrentCulture = IsSupportedCulture(CultureInfo.CurrentUICulture)
            ? CultureInfo.CurrentUICulture
            : CultureInfo.GetCultureInfo("en-US");

        CultureInfo.CurrentCulture = CurrentCulture;
        CultureInfo.CurrentUICulture = CurrentCulture;
        CultureInfo.DefaultThreadCurrentCulture = CurrentCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CurrentCulture;
    }

    public void SetCulture(string cultureName)
    {
        var culture = IsSupportedCultureName(cultureName)
            ? CultureInfo.GetCultureInfo(cultureName)
            : CultureInfo.GetCultureInfo("en-US");
        if (Equals(CurrentCulture, culture)) return;
        CurrentCulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureChanged?.Invoke();
    }

    private static bool IsSupportedCultureName(string cultureName) =>
        cultureName.Equals("en-US", StringComparison.OrdinalIgnoreCase)
        || cultureName.Equals("pt-BR", StringComparison.OrdinalIgnoreCase);

    private static bool IsSupportedCulture(CultureInfo culture) =>
        IsSupportedCultureName(culture.Name);
}
