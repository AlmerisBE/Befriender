namespace Befriender.UI.Theme.Contracts;

using Befriender.UI.Theme.Models;

public interface IThemeService {
    ThemePalette CurrentPalette { get; }
    ThemeStyle CurrentStyle { get; }
    void SetTheme(ThemeStyle style);
}