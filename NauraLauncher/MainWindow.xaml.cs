using System.Windows;
using System.Windows.Input;

namespace NauraLauncher;

/// <summary>
/// Interaction logic for MainWindow.xaml — hosts the APEX marketplace shell
/// and provides custom chrome (min / max / close) behaviour.
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void MinBtn_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState.Minimized;

    private void MaxBtn_Click(object sender, RoutedEventArgs e)
        => WindowState = WindowState == WindowState.Maximized
            ? WindowState.Normal
            : WindowState.Maximized;

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
        => Close();
}
