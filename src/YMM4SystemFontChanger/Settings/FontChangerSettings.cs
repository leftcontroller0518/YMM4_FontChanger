using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using YMM4SystemFontChanger.Services;
using YukkuriMovieMaker.Plugin;

namespace YMM4SystemFontChanger.Settings;

/// <summary>
/// 「YMM4 System Font Changer」プラグインの設定です。
/// <see cref="SettingsBase{T}"/>を継承することで、YMM4本体の公開APIのみを用いて
/// 設定のロード・保存(永続化)・「設定」→「プラグイン」画面への統合を実現します。
///
/// この設定クラスは、動画内のテキスト/字幕のフォントには一切関与しません。
/// あくまでYMM4本体アプリケーションのUI(WPF)表示フォントのみを対象とします。
/// </summary>
public sealed class FontChangerSettings : SettingsBase<FontChangerSettings>, ISetting
{
    /// <inheritdoc />
    public override SettingsCategory Category => SettingsCategory.None;

    /// <inheritdoc />
    public override string Name => "System Font Changer";

    /// <inheritdoc />
    public override bool HasSettingView => true;

    /// <summary>
    /// 設定ビュー(UserControl)を返します。
    /// DataContextには、プロセス内で唯一の正となる <see cref="SettingsBase{T}.Default"/> を割り当てます。
    /// これにより、YMM4が生成する設定用インスタンスと、プラグイン起動時にフォントを適用する
    /// インスタンスが必ず一致し、設定内容の不整合を防ぎます。
    /// </summary>
    public override object SettingView => new FontChangerSettingsView
    {
        DataContext = Default
    };

    private string? selectedFontFamilyName;

    /// <summary>
    /// 設定の初期化処理です。
    /// nullの場合はYMM4本来の既定フォントを使用します。
    /// </summary>
    public override void Initialize()
    {
        selectedFontFamilyName = null;
    }

    /// <summary>
    /// 現在選択されているフォントファミリー名。
    /// null または空文字の場合は「YMM4本来の既定フォント」を意味します。
    /// </summary>
    public string? SelectedFontFamilyName
    {
        get => selectedFontFamilyName;
        private set
        {
            if (selectedFontFamilyName == value)
            {
                return;
            }

            selectedFontFamilyName = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsCustomFontApplied));
        }
    }

    /// <summary>
    /// カスタムフォントが適用中かどうか(設定画面のボタン活性状態などに使用)。
    /// </summary>
    public bool IsCustomFontApplied =>
        !string.IsNullOrWhiteSpace(SelectedFontFamilyName);

    /// <summary>
    /// Windowsにインストールされているフォントの一覧(表示名)。
    /// 設定画面のコンボボックスの選択肢として使用します。
    /// </summary>
    public IReadOnlyList<string> AvailableFontFamilyNames { get; } =
        InstalledFontProvider.GetInstalledFontFamilyNames();

    /// <summary>
    /// 指定したフォントをYMM4本体のUIへ即座に適用し、設定を永続化します。
    /// 設定画面からユーザーがフォントを選択した際に呼び出されます。
    /// </summary>
    public void ApplyFont(string? fontFamilyName)
    {
        SelectedFontFamilyName = fontFamilyName;
        FontOverrideService.SetFontFamily(fontFamilyName);
        Save();
    }

    /// <summary>
    /// フォントの上書きを解除し、YMM4本来の既定フォント表示に戻します。
    /// </summary>
    public void ResetToDefault()
    {
        ApplyFont(null);
    }

    /// <summary>
    /// YMM4起動時に、保存済みのフォント設定をUIへ反映します。
    /// プラグイン本体(<see cref="SystemFontChangerPlugin"/>)から一度だけ呼び出されます。
    /// </summary>
    public void ApplySavedFontOnStartup()
    {
        FontOverrideService.SetFontFamily(SelectedFontFamilyName);
    }
}