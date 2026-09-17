# 軽量化・リファクタリング・UX大幅向上の実装計画

本ドキュメントでは、「YMM4 System Font Changer」プラグインにおけるパフォーマンス軽量化、コード品質向上（リファクタリング）、および設定画面のUX大幅向上のための設計と変更方針を定義します。

---

## ユーザー確認・検討事項

> [!NOTE]
> 設定画面をより直感的かつ快適に操作できるようにするため、以下の機能強化を提案します。
> 1. **フォント自体の書体で表示されるプレビュー付きドロップダウン + ライブ見本カード**
> 2. **日本語対応（和文）フォント絞り込みトグル & リアルタイム検索**
> 3. **UI仮想化（VirtualizingStackPanel）による超高速スクロール & 非同期フォント一覧読み込み**
> 4. **MVVMクリーン設計へのリファクタリング**

---

## 提案する変更内容

### 1. 軽量化 & パフォーマンス最適化

- **フォント情報の非同期ロード & 遅延読み込み**:
  - `InstalledFontProvider` でのフォント走査をバックグラウンドスレッド (`Task.Run`) で非同期実行。
  - プラグイン読み込み時・設定画面表示時のUIフリーズ（ブロッキング）を完全に排除。
  - 読み込み中はモダンなインジケーター（「フォント一覧を読み込み中...」）を表示。
- **UI仮想化 (`VirtualizingStackPanel`) の導入**:
  - 数百〜数千のフォントがインストールされている環境でも、メモリ消費を最小限に抑え、ドロップダウン展開時やスクロールを 60fps で超高速・スムーズに動作させます。
- **`FontOverrideService` のイベント負荷低減**:
  - フォント未適用時（デフォルト状態）は `Loaded` イベントハンドラ内の処理を即時 return してオーバーヘッドを極小化。
  - プロパティ値の比較を行い、変更がない要素に対する不要なプロパティ再設定を回避。

---

### 2. UX の大幅向上

- **各フォント自身の書体でプレビュー表示されるドロップダウン**:
  - リスト項目にそのフォントファミリーを適用し、フォント名がそのフォント自身の字体でプレビュー表示されます。
- **リアルタイム見本プレビュー枠（Live Preview Box）**:
  - 設定画面下部に「リアルタイム見本」カードを配置。
  - 「あいうえお 漢字 ABC abc 0123 - サンプルテキスト」を表示し、選択したフォントが実際にどのように見えるかを確認可能。
- **日本語対応（和文）フォント絞り込み & 検索フィルター**:
  - 「和文フォントのみ表示」チェックボックスを追加（英字専用フォントと日本語フォントを素早く切り替え）。
  - フォント名検索入力ボックス（インクリメンタルサーチ）により、目的のフォントを瞬時に特定。
- **ステータスバッジ & YMM4調和モダンデザイン**:
  - 「現在適用中: (フォント名)」/「既定フォント使用中」の視覚的バッジ表示。
  - 余白・フォントサイズ・ボーダー・配色を整理し、YMM4のダーク/ライトテーマに美しく馴染むデザイン。

---

### 3. リファクタリング (コード品質・保守性)

- **MVVM パターンの徹底**:
  - 設定クラス `FontChangerSettings` は設定の永続化に専念。
  - 設定画面用のデータモデル `FontItem`、汎用 `RelayCommand` を新設。
  - View のコードビハインドにおけるイベント処理（`SelectionChanged`、`Click` など）をすべて XAML バインディングと Command へ移行。
- **和文フォント判定ロジックの追加**:
  - `FontFamily.FamilyNames` やカルチャ情報をもとに、和文フォントを高速・安全に自動判定。

---

## 変更対象ファイル一覧

| 区分 | ファイルパス | 説明 |
| :--- | :--- | :--- |
| **MODIFY** | [`InstalledFontProvider.cs`](file:///D:/YMM4plugins/YMM4SystemFontChanger/YMM4SystemFontChanger/src/YMM4SystemFontChanger/Services/InstalledFontProvider.cs) | 非同期読み込み、和文フォント判定、FontFamilyモデル生成 |
| **MODIFY** | [`FontOverrideService.cs`](file:///D:/YMM4plugins/YMM4SystemFontChanger/YMM4SystemFontChanger/src/YMM4SystemFontChanger/Services/FontOverrideService.cs) | 負荷低減・不要な再走査のスキップ |
| **MODIFY** | [`FontChangerSettings.cs`](file:///D:/YMM4plugins/YMM4SystemFontChanger/YMM4SystemFontChanger/src/YMM4SystemFontChanger/Settings/FontChangerSettings.cs) | 設定永続化の整理、非同期ロード連携、フィルター・検索プロパティ |
| **MODIFY** | [`FontChangerSettingsView.xaml`](file:///D:/YMM4plugins/YMM4SystemFontChanger/YMM4SystemFontChanger/src/YMM4SystemFontChanger/Settings/FontChangerSettingsView.xaml) | プレビュードロップダウン、仮想化、検索・和文フィルター、見本エリア、モダンレイアウト |
| **MODIFY** | [`FontChangerSettingsView.xaml.cs`](file:///D:/YMM4plugins/YMM4SystemFontChanger/YMM4SystemFontChanger/src/YMM4SystemFontChanger/Settings/FontChangerSettingsView.xaml.cs) | コードビハインドのクリーン化 |
| **NEW** | [`RelayCommand.cs`](file:///D:/YMM4plugins/YMM4SystemFontChanger/YMM4SystemFontChanger/src/YMM4SystemFontChanger/Common/RelayCommand.cs) | MVVM用汎用コマンド実装 |
| **NEW** | [`FontItem.cs`](file:///D:/YMM4plugins/YMM4SystemFontChanger/YMM4SystemFontChanger/src/YMM4SystemFontChanger/Models/FontItem.cs) | フォント表示名・FontFamily・和文フラグ等を保持するデータモデル |

---

## 検証計画

### 1. ビルド検証
- `dotnet build` を実行し、警告 0、エラー 0 でコンパイルできることを確認。

### 2. 機能・UX・パフォーマンステスト
- フォント一覧が非同期でスムーズにロードされること。
- ドロップダウンでのフォントプレビュー表示および UI 仮想化による高速スクロール。
- 和文フォント絞り込みトグルおよび検索テキストボックスが高速に動作すること。
- フォント選択時にYMM4のUIに即座に反映され、見本プレビューも連動すること。
- 「デフォルトに戻す」ボタンで正常に既定フォントへリセットされること。
