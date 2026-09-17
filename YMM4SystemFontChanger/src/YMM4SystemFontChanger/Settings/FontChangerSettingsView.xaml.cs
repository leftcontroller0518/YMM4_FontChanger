using System.Windows.Controls;

namespace YMM4SystemFontChanger.Settings;

/// <summary>
/// 「System Font Changer」の設定画面(YMM4の「設定」→「プラグイン」から表示されます)。
/// </summary>
public partial class FontChangerSettingsView : UserControl
{
    public FontChangerSettingsView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// フォント選択が変更された際、即座にYMM4本体のUIへ反映して保存します。
    /// </summary>
    private void FontComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (DataContext is not FontChangerSettings settings)
        {
            return;
        }

        var selected = FontComboBox.SelectedItem as string
            ?? (string.IsNullOrWhiteSpace(FontComboBox.Text) ? null : FontComboBox.Text);

        settings.ApplyFont(selected);
    }

    /// <summary>
    /// 「デフォルトに戻す」ボタン押下時、フォントの上書きを解除します。
    /// </summary>
    private void ResetButton_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is not FontChangerSettings settings)
        {
            return;
        }

        settings.ResetToDefault();
        FontComboBox.SelectedItem = null;
        FontComboBox.Text = string.Empty;
    }
}
