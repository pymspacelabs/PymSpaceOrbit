namespace PymGameOrbit.Shared.Abstractions.Localization;

public interface ILocalizer<T>
{
    string this[string key] { get; }
}