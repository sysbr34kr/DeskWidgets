
using System;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;

namespace DeskWidgets.Services;

public static class LocalizationService
{
    public const string DefaultLanguage = "en-US";
    public static event EventHandler? LanguageChanged;

    private static ResourceDictionary? _currentDictionary;

    public static string CurrentLanguage { get; private set; } =
        DefaultLanguage;

    public static string NormalizeLanguage(string? languageId)
    {
        return languageId is "en-US" or "pt-BR"
            ? languageId
            : DefaultLanguage;
    }

    public static void Apply(string? languageId)
    {
        string language = NormalizeLanguage(languageId);

        var application = Application.Current;

        if (application is null)
            return;

        string resourcePath =
            $"Localization/Strings.{language}.xaml";

        var dictionaries =
            application.Resources.MergedDictionaries;

        var dictionary = new ResourceDictionary
        {
            Source = new Uri(
                $"/DeskWidgets;component/{resourcePath}",
                UriKind.Relative)
        };

        if (_currentDictionary is not null)
        {
            dictionaries.Remove(_currentDictionary);
        }
        else
        {
            var existing = dictionaries.FirstOrDefault(
                d => d.Source?.OriginalString.Contains(
                    "Localization/Strings.",
                    StringComparison.OrdinalIgnoreCase) == true);

            if (existing is not null)
                dictionaries.Remove(existing);
        }

        dictionaries.Add(dictionary);

        _currentDictionary = dictionary;
        CurrentLanguage = language;

        var culture = CultureInfo.GetCultureInfo(language);

        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;

        Thread.CurrentThread.CurrentCulture = culture;
        Thread.CurrentThread.CurrentUICulture = culture;

        LanguageChanged?.Invoke(null, EventArgs.Empty);
    }

    public static string GetString(string key)
    {
        return Application.Current.TryFindResource(key)
            as string ?? key;
    }

    public static string Format(
        string resourceKey,
        params object?[] arguments)
    {
        string template = GetString(resourceKey);

        return string.Format(
            CultureInfo.GetCultureInfo(CurrentLanguage),
            template,
            arguments);
    }
}
