using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace NauraLauncher.Controls;

/// <summary>
/// A tiny reusable outline-icon renderer. Point it at a Geometry defined in
/// Icons.xaml via <see cref="Data"/> and it draws a stroked Lucide-style glyph
/// scaled into a 24x24 viewbox. Set <see cref="Filled"/> for solid glyphs.
/// </summary>
public class IconControl : FrameworkElement
{
    public static readonly DependencyProperty DataProperty =
        DependencyProperty.Register(nameof(Data), typeof(Geometry), typeof(IconControl),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender));

    public Geometry? Data
    {
        get => (Geometry?)GetValue(DataProperty);
        set => SetValue(DataProperty, value);
    }

    public static readonly DependencyProperty BrushProperty =
        DependencyProperty.Register(nameof(Brush), typeof(Brush), typeof(IconControl),
            new FrameworkPropertyMetadata(Brushes.White, FrameworkPropertyMetadataOptions.AffectsRender));

    public Brush Brush
    {
        get => (Brush)GetValue(BrushProperty);
        set => SetValue(BrushProperty, value);
    }

    public static readonly DependencyProperty StrokeWidthProperty =
        DependencyProperty.Register(nameof(StrokeWidth), typeof(double), typeof(IconControl),
            new FrameworkPropertyMetadata(2.0, FrameworkPropertyMetadataOptions.AffectsRender));

    public double StrokeWidth
    {
        get => (double)GetValue(StrokeWidthProperty);
        set => SetValue(StrokeWidthProperty, value);
    }

    public static readonly DependencyProperty FilledProperty =
        DependencyProperty.Register(nameof(Filled), typeof(bool), typeof(IconControl),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsRender));

    public bool Filled
    {
        get => (bool)GetValue(FilledProperty);
        set => SetValue(FilledProperty, value);
    }

    protected override void OnRender(DrawingContext dc)
    {
        if (Data is null) return;

        double size = Math.Min(ActualWidth, ActualHeight);
        if (size <= 0) return;

        // Icons are authored in a 24x24 space.
        double scale = size / 24.0;

        dc.PushTransform(new TranslateTransform(
            (ActualWidth - size) / 2, (ActualHeight - size) / 2));
        dc.PushTransform(new ScaleTransform(scale, scale));

        var pen = new Pen(Brush, StrokeWidth)
        {
            LineJoin = PenLineJoin.Round,
            StartLineCap = PenLineCap.Round,
            EndLineCap = PenLineCap.Round,
        };
        pen.Freeze();

        dc.DrawGeometry(Filled ? Brush : null, Filled ? null : pen, Data);

        dc.Pop();
        dc.Pop();
    }
}
