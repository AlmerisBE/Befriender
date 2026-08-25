namespace Befriender.UI.Theme.Contracts;

using Befriender.UI.Theme.Models;
using System.Collections.Generic;

public interface IThemeService {
    ThemePalette CurrentPalette { get; }
    string CurrentThemeName { get; }
    IReadOnlyList<string> GetAvailableThemes();
    void SetTheme(string themeName);
}