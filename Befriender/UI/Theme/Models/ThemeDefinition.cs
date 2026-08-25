namespace Befriender.UI.Theme.Models;

public class ThemeDefinition {
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public ThemePalette Palette { get; set; } = new();
}