using System;
using System.Windows;
using System.Windows.Media;

namespace NauraLauncher;

/// <summary>
/// Interaction logic for MainWindow.xaml — hosts the APEX page shell and
/// provides custom chrome (min / max / close) plus the rounded-corner clip.
/// </summary>
public partial class MainWindow : Window
{
    /// <summary>Corner radius of the frameless window while it is restored.</summary>
    private const double WindowCornerRadius = 16.0;

    public MainWindow()
    {
        InitializeComponent();

        // The clip depends on both the radius and the current size, so keep it
        // in sync with every resize and every Normal <-> Maximized transition.
        SizeChanged += MainWindow_SizeChanged;
        StateChanged += MainWindow_StateChanged;

        ApplyWindowCorners(RenderSize);
    }

    private void MinBtn_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaxBtn_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
        => Close();

    private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        => ApplyWindowCorners(e.NewSize);

    private void MainWindow_StateChanged(object? sender, EventArgs e)
        => ApplyWindowCorners(RenderSize);

    /// <summary>
    /// Rounds the shell to <see cref="WindowCornerRadius"/> and squares it off
    /// when maximized (Windows hides the desktop corners behind the window, so
    /// a rounded clip there would leave transparent notches).
    /// </summary>
    private void ApplyWindowCorners(Size size)
    {
        double radius = WindowState == WindowState.Maximized ? 0.0 : WindowCornerRadius;

        RootBorder.CornerRadius = new CornerRadius(radius);
        RootBorder.Clip = CreateCornerClip(radius, size);
    }

    /// <summary>
    /// A rounded rect matching the root border, so page content (hero art,
    /// thumbnails) is clipped to the same shape as the background.
    /// </summary>
    private static Geometry? CreateCornerClip(double radius, Size size)
    {
        if (radius <= 0 || size.Width <= 0 || size.Height <= 0) return null;

        var clip = new RectangleGeometry(new Rect(0, 0, size.Width, size.Height), radius, radius);
        clip.Freeze();
        return clip;
    }
}
