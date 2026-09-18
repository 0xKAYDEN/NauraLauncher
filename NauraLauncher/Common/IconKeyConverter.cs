using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace NauraLauncher.Common;

/// <summary>
/// Resolves a string resource key (e.g. "Icon.Gauge") into the corresponding
/// <see cref="Geometry"/> stored in the merged application resource dictionaries.
/// Lets model data reference icons by name.
/// </summary>
public class IconKeyConverter : IValueConverter
{
    public static readonly IconKeyConverter Instance = new();

    public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string key && System.Windows.Application.Current?.TryFindResource(key) is Geometry g)
            return g;
        return null;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
