namespace NauraLauncher.Models;

/// <summary>
/// A compact metric tile shown along the top of the Home page.
/// </summary>
public class StatCard
{
    public string IconKey { get; set; } = string.Empty;    // resource key in Icons.xaml
    public string Label { get; set; } = string.Empty;      // e.g. "PLAYTIME THIS WEEK"
    public string Value { get; set; } = string.Empty;      // e.g. "12H 40M"
    public string Delta { get; set; } = string.Empty;      // e.g. "+3H VS LAST WEEK"
    public bool DeltaIsPositive { get; set; } = true;
}
