using  PymGameOrbit.Shared.Abstractions.Localization;
using System.Globalization;
using Microsoft.Extensions.Localization;
namespace PymGameOrbit.Shared.Concrete.Localization;

public class Localizer<T>(IStringLocalizerFactory factory) : ILocalizer<T> 
{
    private readonly IStringLocalizer _localizer =
        factory.Create("Strings", typeof(T).Assembly.FullName!);

    public string this[string key] => _localizer[key];
}
