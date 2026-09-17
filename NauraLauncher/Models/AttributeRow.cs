namespace NauraLauncher.Models;

/// <summary>
/// One row of the auction "Provenance &amp; Attributes" table.
/// </summary>
public class AttributeRow
{
    public string Label { get; set; } = string.Empty;      // e.g. "FORGE DATE"
    public string Value { get; set; } = string.Empty;      // e.g. "14 · 09 · 2089"
    public bool IsAccent { get; set; }                     // rendered in accent green
    public string? MonoValue { get; set; }                 // when set, rendered in the mono face
}
