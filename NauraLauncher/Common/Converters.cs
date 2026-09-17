using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NauraLauncher.Common;

/// <summary>Returns Visible when the bound string is non-empty, else Collapsed.</summary>
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => string.IsNullOrWhiteSpace(value as string) ? Visibility.Collapsed : Visibility.Visible;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>Returns Visible when bool is true (invert with parameter "invert").</summary>
public class BoolToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool b = value is true;
        if (string.Equals(parameter as string, "invert", StringComparison.OrdinalIgnoreCase)) b = !b;
        return b ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Turns a 0..1 fraction into a star <see cref="GridLength"/>. Pair it with a
/// two-column track grid so progress fills stay responsive instead of being
/// hard-coded pixel widths.
/// </summary>
public class PercentToGridLengthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        double fraction = value switch
        {
            double d => d,
            float f => f,
            int i => i,
            string s when double.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out double parsed) => parsed,
            _ => 0d,
        };

        if (double.IsNaN(fraction) || double.IsInfinity(fraction)) fraction = 0d;
        fraction = Math.Clamp(fraction, 0d, 1d);
        return new GridLength(fraction, GridUnitType.Star);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// True when the bound value equals <c>ConverterParameter</c>. Used to drive
/// segmented controls: <c>IsChecked="{Binding Mode, Converter={StaticResource ParamMatch},
/// ConverterParameter=Borderless}"</c>. Writing back pushes the parameter onto
/// the bound property when the option becomes checked.
/// </summary>
public class ParameterMatchConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => Equals(value, parameter);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? parameter! : Binding.DoNothing;
}

/// <summary>
/// Visible when the bound value equals <c>ConverterParameter</c>. Used to swap
/// the Settings content pane as the left-hand sections rail changes.
/// </summary>
public class ParameterMatchToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => Equals(value, parameter) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

/// <summary>
/// Resolves a hex colour string from model data (e.g. "#FFB020") into a frozen
/// <see cref="Brush"/>, so rarity tones live in the view models instead of the
/// XAML.
/// </summary>
public class StringToBrushConverter : IValueConverter
{
    private static readonly BrushConverter BrushConverter = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is not string hex || string.IsNullOrWhiteSpace(hex)) return null;

        try
        {
            return BrushConverter.ConvertFromString(hex) as Brush;
        }
        catch (FormatException)
        {
            return null;
        }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
