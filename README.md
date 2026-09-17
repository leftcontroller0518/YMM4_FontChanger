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

## ビルド方法

1. [Visual Studio](https://visualstudio.microsoft.com/ja/)（.NET 10 SDK を含む最新版）をインストールする
2. `Directory.Build.props.sample` をコピーし、同じ階層に `Directory.Build.props` として保存する
3. `Directory.Build.props` を開き、`YMM4DirPath` に **お使いのYMM4のインストールフォルダ** の絶対パスを設定する
   （末尾は必ず `\` で終わること。例: `D:\YMM4\`）
   - 開発中に本体を破損させないよう、普段使いとは別の検証用YMM4フォルダを用意することを推奨します
4. `YMM4SystemFontChanger.sln` を Visual Studio で開き、ビルドする
   - ビルド後、自動的に `$(YMM4DirPath)user\plugin\YMM4SystemFontChanger.dll` へコピーされます
     （`YukkuriMovieMaker.exe` が起動中の場合、DLLがロックされコピーに失敗することがあります。
     ビルド前にYMM4を終了してください）
5. YMM4を起動し、「設定」→「プラグイン」→「プラグイン一覧」に
   **YMM4 System Font Changer** が表示されていれば読み込み成功です
6. 「設定」の左側リストから **System Font Changer** を選択し、フォントを選んでください

### デバッグ方法

1. `YMM4SystemFontChanger` プロジェクトを右クリック→「スタートアッププロジェクトに設定」
2. デバッグプロパティで実行可能ファイルに `$(YMM4DirPath)YukkuriMovieMaker.exe` を指定する
3. デバッグ実行すると、YMM4本体にアタッチした状態でプラグインの動作を確認できます

## 配布用パッケージ化（.ymme）

1. ビルドで生成された `YMM4SystemFontChanger.dll`（と`.pdb`があれば同梱）をzip圧縮する
2. 拡張子を `.ymme` に変更する
3. 配布する（ダブルクリックでワンクリックインストール可能）

## 実装メモ

- **フォント適用の仕組み**: `FontOverrideService` が、
  1. `EventManager.RegisterClassHandler` で `FrameworkElement`/`FrameworkContentElement` の
     `Loaded` イベントをアプリ全体で購読し、以後生成される全てのUI要素（新規ダイアログ・
     コンテキストメニュー・ポップアップ等を含む）に自動でフォントを適用
  2. 既に表示中のウィンドウについては `VisualTreeHelper`/`LogicalTreeHelper` で
     ツリーを再帰的に辿り、その場で `Control.FontFamilyProperty` /
     `TextBlock.FontFamilyProperty` / `TextElement.FontFamilyProperty` へ
     `SetValue`（適用時）/ `ClearValue`（解除時）することで即時反映を実現しています。
  - `FrameworkPropertyMetadata.OverrideMetadata` によるアプリ全体既定値の書き換えは、
    「型が一度でも使用された後に呼び出すと例外が発生する」という制約上、
    呼び出しタイミングを保証できず不安定になるため採用していません。
    上記の局所値ベースの方式であれば、いつ呼び出しても安全かつ「デフォルトに戻す」も
    確実に実現できます。
- **設定の一貫性**: `FontChangerSettings`（`SettingsBase<T>`継承）の `SettingView` は、
  YMM4のプラグインローダーが生成する個別インスタンスではなく、
  プロセス内で共有される `SettingsBase<T>.Default` を `DataContext` に割り当てています。
  これにより、起動時にフォントを適用する処理と設定画面での操作が常に同じ状態を参照し、
  不整合が起きないようにしています。
- **動画内テキストとの分離**: 本プラグインが操作するのは WPF コントロールの
  `FontFamily` 依存関係プロパティのみです。YMM4のプレビュー/出力に使われる
  テキスト・字幕はDirect2D/DirectWriteで別途描画されるため、本プラグインの
  操作対象には含まれず、意図せず変更されることはありません。

## 動作要件

- YukkuriMovieMaker4 v4.47以降
- .NET 10 Desktop Runtime（YMM4本体と共通）
- Windows

## ライセンス

MIT License（本サンプルはご自由に改変・再配布いただけます）
