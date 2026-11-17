using System.Windows;
using System.Windows.Controls;

namespace DeskWidgets.Controls;

public partial class SettingToggle : UserControl
{
    public SettingToggle()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(SettingToggle),
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    public static readonly DependencyProperty IsCheckedProperty =
        DependencyProperty.Register(
            nameof(IsChecked),
            typeof(bool),
            typeof(SettingToggle),
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public bool IsChecked
    {
        get => (bool)GetValue(IsCheckedProperty);
        set => SetValue(IsCheckedProperty, value);
    }
}
