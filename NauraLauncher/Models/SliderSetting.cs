using System.Globalization;
using NauraLauncher.Common;

namespace NauraLauncher.Models;

/// <summary>
/// A labelled shadcn Slider row. <see cref="Format"/> is a composite format
/// string applied to the raw value to build the live read-out
/// (e.g. "{0:0}%" or "{0:0} FPS").
/// </summary>
public class SliderSetting : ObservableObject
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Minimum { get; set; }
    public double Maximum { get; set; } = 100;
    public double TickFrequency { get; set; } = 1;
    public string Format { get; set; } = "{0:0}";

    private double _value;
    public double Value
    {
        get => _value;
        set
        {
            if (!SetProperty(ref _value, value)) return;
            OnPropertyChanged(nameof(ValueLabel));
            OnPropertyChanged(nameof(Fraction));
        }
    }

    /// <summary>Fractional position (0..1) for progress-style fills.</summary>
    public double Fraction => Maximum <= Minimum
        ? 0
        : Math.Clamp((Value - Minimum) / (Maximum - Minimum), 0, 1);

    public string ValueLabel => string.Format(CultureInfo.InvariantCulture, Format, Value);
}
