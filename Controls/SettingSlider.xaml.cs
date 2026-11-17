using System.Windows;
using System.Windows.Controls;

namespace DeskWidgets.Controls;

public partial class SettingSlider : UserControl
{
    public SettingSlider()
    {
        InitializeComponent();
    }

    // Setting Label
    public static readonly DependencyProperty LabelProperty =
        DependencyProperty.Register(
            nameof(Label),
            typeof(string),
            typeof(SettingSlider),
            new PropertyMetadata(string.Empty));

    public string Label
    {
        get => (string)GetValue(LabelProperty);
        set => SetValue(LabelProperty, value);
    }

    // Current Value
    public static readonly DependencyProperty ValueProperty =
        DependencyProperty.Register(
            nameof(Value),
            typeof(double),
            typeof(SettingSlider),
            new FrameworkPropertyMetadata(
                36.0,
                FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public double Value
    {
        get => (double)GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    // Minimum Value
    public static readonly DependencyProperty MinimumProperty =
        DependencyProperty.Register(
            nameof(Minimum),
            typeof(double),
            typeof(SettingSlider),
            new PropertyMetadata(0.0));

    public double Minimum
    {
        get => (double)GetValue(MinimumProperty);
        set => SetValue(MinimumProperty, value);
    }

    // Maximum Value
    public static readonly DependencyProperty MaximumProperty =
        DependencyProperty.Register(
            nameof(Maximum),
            typeof(double),
            typeof(SettingSlider),
            new PropertyMetadata(100.0));

    public double Maximum
    {
        get => (double)GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }

    // Slider Step
    public static readonly DependencyProperty StepProperty =
        DependencyProperty.Register(
            nameof(Step),
            typeof(double),
            typeof(SettingSlider),
            new PropertyMetadata(1.0));

    public double Step
    {
        get => (double)GetValue(StepProperty);
        set => SetValue(StepProperty, value);
    }
}
