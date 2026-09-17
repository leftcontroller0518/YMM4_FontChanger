# YMM4 System Font Changer

YukkuriMovieMaker4（YMM4）本体のUI表示フォント（メニュー・ボタン・パネル・設定画面・タイムライン等）を
変更するプラグインです。**動画内のテキスト・字幕のフォント設定には一切影響しません。**

- 対象バージョン: YMM4 **v4.47以降**（.NET 10 + WPF移行後）
- 使用API: `YukkuriMovieMaker.Plugin.dll` の公開APIのみ
  - `YukkuriMovieMaker.Plugin.IPlugin`
  - `YukkuriMovieMaker.Plugin.ISetting` / `YukkuriMovieMaker.Plugin.SettingsBase<T>`
  - YMM4本体の非公開実装（`YukkuriMovieMaker.exe`内部の型や`YukkuriMovieMaker.Controls.dll`等）には依存しません
- フォント適用は WPF標準の公開API（`DependencyObject.SetValue/ClearValue`, `EventManager.RegisterClassHandler`,
  `VisualTreeHelper`, `LogicalTreeHelper` 等）のみで実現しており、リフレクションによる内部フィールド操作や
  P/Invokeは一切使用していません。

## 主な機能

| 項目 | 内容 |
| --- | --- |
| フォント選択 | Windowsにインストールされている全フォントから選択可能（コンボボックス、手入力補完対応） |
| 適用範囲 | メニュー・ボタン・パネル・設定画面・タイムライン等、YMM4本体のUI全体（起動中に開かれる新規ウィンドウ・ダイアログ・ポップアップメニューにも自動追従） |
| 設定画面 | YMM4の「設定」→「プラグイン」から表示・変更可能 |
| 永続化 | 選択したフォントは自動的に保存され、次回起動時も自動適用 |
| デフォルトに戻す | ワンクリックでYMM4本来のフォント表示に戻せます |
| 即時反映 | フォント選択時、再起動不要でその場でUIへ反映されます |
| 対象外 | 動画内テキスト・字幕のフォント設定（Direct2D/DirectWriteで別途描画されるため、本プラグインの対象外） |

## プロジェクト構成

```
YMM4SystemFontChanger/
├─ YMM4SystemFontChanger.sln
├─ Directory.Build.props.sample   ← コピーして Directory.Build.props にし、YMM4のパスを設定する
├─ .gitignore
└─ src/
   └─ YMM4SystemFontChanger/
      ├─ YMM4SystemFontChanger.csproj
      ├─ SystemFontChangerPlugin.cs        … IPlugin実装（プラグイン本体・起動時のフォント適用）
      ├─ Settings/
      │  ├─ FontChangerSettings.cs         … ISetting実装（設定の保持・永続化・適用ロジック）
      │  ├─ FontChangerSettingsView.xaml   … 設定画面のXAML
      │  └─ FontChangerSettingsView.xaml.cs
      └─ Services/
         ├─ FontOverrideService.cs         … UI全体へのフォント適用/解除を行うコアロジック
         └─ InstalledFontProvider.cs       … インストール済みフォント一覧の取得
```

## ライセンス

MIT License（本サンプルはご自由に改変・再配布いただけます）
