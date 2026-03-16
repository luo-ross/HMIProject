using RS.SetupApp.Core;

namespace RS.SetupApp.ViewModels;

public static class LocalizedManifestTextResolver
{
    public static string Resolve(LocalizedTextManifest? value, string fallback, UiLanguage language)
    {
        if (value == null)
        {
            return fallback;
        }

        string? candidate = language switch
        {
            UiLanguage.ChineseSimplified => value.ZhCn,
            _ => value.EnUs
        };

        if (!string.IsNullOrWhiteSpace(candidate))
        {
            return candidate!;
        }

        if (!string.IsNullOrWhiteSpace(value.Default))
        {
            return value.Default!;
        }

        return fallback;
    }
}
