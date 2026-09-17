using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace YMM4SystemFontChanger.Services;

/// <summary>
/// YMM4本体のWPF UI(メニュー・ボタン・パネル・設定画面・タイムライン等)に対して、
/// フォントファミリーを一括適用/解除するサービスです。
///
/// 実装方針:
/// - WPFの公開API(DependencyObject.SetValue / ClearValue, EventManager.RegisterClassHandler,
///   VisualTreeHelper など)のみを使用し、YMM4本体の非公開型やリフレクションによる
///   内部フィールド操作には一切依存しません。
/// - FrameworkElement / FrameworkContentElement の Loaded イベントをアプリ全体で購読することで、
///   プラグインロード時点で存在しないウィンドウやダイアログ、メニュー、ポップアップ等
///   「後から生成される」UI要素にも自動的にフォントを適用します。
/// - 既に画面に表示されている要素については、ビジュアルツリーを再帰的に辿って
///   その場でフォントを更新することで、設定変更を即座に反映します。
/// - あくまで WPF コントロールの FontFamily プロパティを操作するだけであり、
///   動画のプレビュー/出力に使われるテキスト・字幕(Direct2D/DirectWriteで別途描画される)には
///   一切干渉しません。
/// </summary>
internal static class FontOverrideService
{
    private static bool _isHooked;
    private static FontFamily? _activeFontFamily;

    /// <summary>
    /// 指定したフォントファミリー名をアプリ全体に適用します。
    /// null または空文字を渡すと、既定のフォント表示に戻します(<see cref="Reset"/>と同義)。
    /// </summary>
    public static void SetFontFamily(string? fontFamilyName)
    {
        _activeFontFamily = string.IsNullOrWhiteSpace(fontFamilyName)
            ? null
            : new FontFamily(fontFamilyName);

        EnsureHooked();
        ApplyToCurrentlyOpenWindows();
    }

    /// <summary>
    /// フォントの上書きを解除し、YMM4本来の既定フォント表示に戻します。
    /// </summary>
    public static void Reset() => SetFontFamily(null);

    /// <summary>
    /// アプリ全体のLoadedイベントを一度だけフックします。
    /// Application.Current が未初期化の起動直後(プラグインコンストラクタ実行時)でも
    /// EventManager.RegisterClassHandler は型ベースの静的登録のため安全に呼び出せます。
    /// </summary>
    private static void EnsureHooked()
    {
        if (_isHooked)
        {
            return;
        }

        _isHooked = true;

        // 通常のコントロール・パネル・ウィンドウ等(FrameworkElement派生)
        EventManager.RegisterClassHandler(
            typeof(FrameworkElement),
            FrameworkElement.LoadedEvent,
            new RoutedEventHandler(OnElementLoaded),
            handledEventsToo: true);

        // RichTextBox/FlowDocument内のRun・Paragraph等(FrameworkContentElement派生)
        EventManager.RegisterClassHandler(
            typeof(FrameworkContentElement),
            FrameworkContentElement.LoadedEvent,
            new RoutedEventHandler(OnElementLoaded),
            handledEventsToo: true);
    }

    private static void OnElementLoaded(object sender, RoutedEventArgs e)
    {
        if (sender is DependencyObject element)
        {
            ApplyToElement(element);
        }
    }

    /// <summary>
    /// 現在開かれている全てのウィンドウ(タイムライン・設定画面・各種パネル等を含む)に対して、
    /// ビジュアルツリーを再帰的に辿りながらフォントを即時適用/解除します。
    /// </summary>
    private static void ApplyToCurrentlyOpenWindows()
    {
        var app = Application.Current;
        if (app is null)
        {
            // アプリケーションがまだ構築されていない起動最初期。
            // この場合は EnsureHooked で登録した Loaded ハンドラが、
            // 以後生成される全てのUI要素に自動的にフォントを適用する。
            return;
        }

        void Apply()
        {
            foreach (Window window in app.Windows)
            {
                ApplyToElement(window);
                WalkAndApply(window);
            }
        }

        if (app.Dispatcher.CheckAccess())
        {
            Apply();
        }
        else
        {
            app.Dispatcher.Invoke(Apply);
        }
    }

    private static void WalkAndApply(DependencyObject parent)
    {
        // 1. ビジュアルツリーの走査 (Visual または Visual3D のみ可能)
        if (IsVisualTreeNode(parent))
        {
            var childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (var i = 0; i < childCount; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                ApplyToElement(child);
                WalkAndApply(child);
            }
        }

        // 2. 論理ツリーの走査 (ビジュアルツリーに含まれない FrameworkContentElement などを対象)
        // ColumnDefinition/RowDefinition 等の純粋な DependencyObject はフォント適用対象外かつ子を持たないため除外
        foreach (var logicalChild in LogicalTreeHelper.GetChildren(parent))
        {
            if (logicalChild is FrameworkContentElement contentElement)
            {
                ApplyToElement(contentElement);
                WalkAndApply(contentElement);
            }
        }
    }

    private static bool IsVisualTreeNode(DependencyObject candidate)
        => candidate is Visual or System.Windows.Media.Media3D.Visual3D;

    /// <summary>
    /// 単一のUI要素に対してフォントを適用、または既定値へ戻します。
    /// 対象外の型(Image, Border等)は何もしません。
    /// </summary>
    private static void ApplyToElement(DependencyObject element)
    {
        try
        {
            switch (element)
            {
                case Control control:
                    SetOrClear(control, Control.FontFamilyProperty);
                    break;
                case TextBlock textBlock:
                    SetOrClear(textBlock, TextBlock.FontFamilyProperty);
                    break;
                case TextElement textElement:
                    SetOrClear(textElement, TextElement.FontFamilyProperty);
                    break;
            }
        }
        catch (Exception)
        {
            // 個別要素での失敗が全体の動作を止めないようにする。
            // (動画内テキスト等、想定外のDependencyObjectに対する保険)
        }
    }

    private static void SetOrClear(DependencyObject target, DependencyProperty property)
    {
        if (_activeFontFamily is null)
        {
            target.ClearValue(property);
        }
        else
        {
            target.SetValue(property, _activeFontFamily);
        }
    }
}
