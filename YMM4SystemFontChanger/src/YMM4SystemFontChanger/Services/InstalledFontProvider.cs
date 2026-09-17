using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Markup;
using System.Windows.Media;

namespace YMM4SystemFontChanger.Services;

/// <summary>
/// Windowsにインストールされているフォントの一覧を提供します。
/// WPFの公開API(<see cref="Fonts.SystemFontFamilies"/>)のみを使用しており、
/// YMM4本体の非公開実装や外部DLLには一切依存しません。
/// </summary>
internal static class InstalledFontProvider
{
    private static readonly XmlLanguage EnglishLanguage = XmlLanguage.GetLanguage("en-US");

    /// <summary>
    /// インストールされているフォントの表示名一覧を、名前順にソートして取得します。
    /// </summary>
    public static IReadOnlyList<string> GetInstalledFontFamilyNames()
    {
        var currentCulture = CultureInfo.CurrentUICulture;
        var currentLanguage = XmlLanguage.GetLanguage(currentCulture.IetfLanguageTag);

        var names = Fonts.SystemFontFamilies
            .Select(family => GetPreferredDisplayName(family, currentLanguage))
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(name => name, StringComparer.Create(currentCulture, ignoreCase: false))
            .ToList();

        return names;
    }

    /// <summary>
    /// フォントファミリーの表示名を、可能な限り現在のUIカルチャに合わせて取得します。
    /// 該当する言語名が無い場合は英語名、それも無ければ最初に見つかった名前にフォールバックします。
    /// </summary>
    private static string GetPreferredDisplayName(FontFamily family, XmlLanguage preferredLanguage)
    {
        if (family.FamilyNames.TryGetValue(preferredLanguage, out var localizedName))
        {
            return localizedName;
        }

        if (family.FamilyNames.TryGetValue(EnglishLanguage, out var englishName))
        {
            return englishName;
        }

        return family.FamilyNames.Values.FirstOrDefault() ?? family.Source;
    }
}
