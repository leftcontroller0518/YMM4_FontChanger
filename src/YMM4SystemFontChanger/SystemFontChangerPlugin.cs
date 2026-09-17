using YMM4SystemFontChanger.Settings;
using YukkuriMovieMaker.Plugin;

namespace YMM4SystemFontChanger;

/// <summary>
/// 「YMM4 System Font Changer」プラグインのエントリポイントです。
/// <see cref="IPlugin"/>を実装することで、YMM4のプラグインローダーに認識され、
/// 「設定」→「プラグイン」の一覧に表示されます。
///
/// 実際のフォント変更設定・UIは<see cref="FontChangerSettings"/>(<see cref="ISetting"/>実装)が担当します。
/// このクラスの役割は、
///   1. プラグインとしての名前・詳細情報をYMM4へ提供すること
///   2. YMM4起動時、保存済みのフォント設定を一度だけUIへ適用すること
/// の2点のみに限定しています。
/// </summary>
[PluginDetails(AuthorName = "YMM4 System Font Changer Contributors")]
public sealed class SystemFontChangerPlugin : IPlugin
{
    /// <inheritdoc />
    public string Name => "YMM4 System Font Changer";

    /// <inheritdoc />
    public PluginDetailsAttribute Details =>
        (PluginDetailsAttribute)System.Attribute.GetCustomAttribute(GetType(), typeof(PluginDetailsAttribute))!;

    /// <summary>
    /// YMM4のプラグインローダーがこの型をインスタンス化するタイミング(アプリ起動時)で、
    /// 保存済みのフォント設定を読み込み、UI全体へ適用します。
    /// </summary>
    public SystemFontChangerPlugin()
    {
        // SettingsBase<T>.Default はプロセス内で共有される正の設定インスタンス。
        // 「設定」→「プラグイン」画面に表示される設定ビューも同じインスタンスを参照するため、
        // ここで読み込んだ状態と設定画面での操作は常に一致します。
        FontChangerSettings.Default.ApplySavedFontOnStartup();
    }
}
