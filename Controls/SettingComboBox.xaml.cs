using System.Collections;
using System.Windows;
using System.Windows.Controls;

namespace DeskWidgets.Controls;

public partial class SettingComboBox : UserControl
{
    public SettingComboBox()
    {
        InitializeComponent();
    }

    // Label
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(SettingComboBox),
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // Available Options
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(
            nameof(ItemsSource),
            typeof(IEnumerable),
            typeof(SettingComboBox),
            new PropertyMetadata(null));

    public IEnumerable? ItemsSource
    {
        get => (IEnumerable?)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    // Selected Value
    public static readonly DependencyProperty SelectedValueProperty =
        DependencyProperty.Register(
            nameof(SelectedValue),
            typeof(object),
            typeof(SettingComboBox),
            new FrameworkPropertyMetadata(
                null,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public object? SelectedValue
    {
        get => GetValue(SelectedValueProperty);
        set => SetValue(SelectedValueProperty, value);
    }

    // Display Property
    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(
            nameof(DisplayMemberPath),
            typeof(string),
            typeof(SettingComboBox),
            new PropertyMetadata(string.Empty));

    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    // Value Property
    public static readonly DependencyProperty SelectedValuePathProperty =
        DependencyProperty.Register(
            nameof(SelectedValuePath),
            typeof(string),
            typeof(SettingComboBox),
            new PropertyMetadata(string.Empty));

    public string SelectedValuePath
    {
        get => (string)GetValue(SelectedValuePathProperty);
        set => SetValue(SelectedValuePathProperty, value);
    }

    // Dropdown Width
    public static readonly DependencyProperty ComboBoxWidthProperty =
        DependencyProperty.Register(
            nameof(ComboBoxWidth),
            typeof(double),
            typeof(SettingComboBox),
            new PropertyMetadata(160.0));

    public double ComboBoxWidth
    {
        get => (double)GetValue(ComboBoxWidthProperty);
        set => SetValue(ComboBoxWidthProperty, value);
    }
}
