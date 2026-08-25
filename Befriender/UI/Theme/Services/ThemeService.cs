namespace Befriender.UI.Theme.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Theme.Models;
using System.Collections.Generic;
using System.Numerics;

public class ThemeService : IThemeService {
    private IConfigurationService configurationService;
    private Dictionary<ThemeStyle, ThemePalette> palettes;

    public ThemePalette CurrentPalette { get; private set; } = null!;
    public ThemeStyle CurrentStyle { get; private set; }

    public ThemeService(IConfigurationService configurationService) {
        this.configurationService = configurationService;
        this.palettes = new Dictionary<ThemeStyle, ThemePalette>();
        this.InitializePalettes();

        var savedStyle = (ThemeStyle)this.configurationService.GetConfig().SelectedTheme;
        this.SetTheme(savedStyle);
    }

    private void InitializePalettes() {
        this.palettes[ThemeStyle.Dark] = new ThemePalette {
            TextOnline = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            TextOffline = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
            TextBusy = new Vector4(0.75f, 0.75f, 0.75f, 1.0f),
            TextArchived = new Vector4(0.45f, 0.45f, 0.6f, 1.0f),
            TextDeleted = new Vector4(0.8f, 0.4f, 0.4f, 1.0f),

            IconDeletedTint = new Vector4(1.0f, 0.2f, 0.2f, 1.0f),
            IconDefaultTint = new Vector4(1.0f, 1.0f, 1.0f, 1.0f), // Pure white for native colors
            IconDimmedTint = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),

            StatusFallbackOnline = new Vector4(0.43f, 0.85f, 0.43f, 1.0f),
            StatusFallbackOffline = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
            StatusFallbackDeleted = new Vector4(0.8f, 0.2f, 0.2f, 1.0f),

            WindowBg = new Vector4(0.15f, 0.14f, 0.14f, 0.95f),
            Text = new Vector4(0.90f, 0.90f, 0.90f, 1.0f),
            ChildBg = new Vector4(0.12f, 0.11f, 0.11f, 0.50f),
            PopupBg = new Vector4(0.15f, 0.14f, 0.14f, 0.95f),
            FrameBg = new Vector4(0.20f, 0.20f, 0.20f, 1.0f),
            FrameBgHovered = new Vector4(0.25f, 0.25f, 0.25f, 1.0f),
            FrameBgActive = new Vector4(0.30f, 0.30f, 0.30f, 1.0f),
            TitleBg = new Vector4(0.12f, 0.11f, 0.11f, 1.0f),
            TitleBgActive = new Vector4(0.20f, 0.15f, 0.15f, 1.0f),
            TitleBgCollapsed = new Vector4(0.10f, 0.10f, 0.10f, 1.0f),
            TableHeaderBg = new Vector4(0.18f, 0.17f, 0.17f, 1.0f),
            TableRowBg = new Vector4(0.15f, 0.14f, 0.14f, 1.0f),
            TableRowBgAlt = new Vector4(0.18f, 0.17f, 0.17f, 1.0f),
            Border = new Vector4(0.30f, 0.25f, 0.25f, 1.0f),
            Tab = new Vector4(0.15f, 0.14f, 0.14f, 1.0f),
            TabHovered = new Vector4(0.25f, 0.20f, 0.20f, 1.0f),
            TabActive = new Vector4(0.30f, 0.25f, 0.25f, 1.0f),
            TabUnfocused = new Vector4(0.12f, 0.11f, 0.11f, 1.0f),
            TabUnfocusedActive = new Vector4(0.18f, 0.17f, 0.17f, 1.0f),
            Button = new Vector4(0.25f, 0.20f, 0.20f, 1.0f),
            ButtonHovered = new Vector4(0.35f, 0.25f, 0.25f, 1.0f),
            ButtonActive = new Vector4(0.40f, 0.30f, 0.30f, 1.0f)
        };

        this.palettes[ThemeStyle.Light] = new ThemePalette {
            TextOnline = new Vector4(0.1f, 0.1f, 0.1f, 1.0f),
            TextOffline = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
            TextBusy = new Vector4(0.4f, 0.4f, 0.4f, 1.0f),
            TextArchived = new Vector4(0.5f, 0.4f, 0.6f, 1.0f),
            TextDeleted = new Vector4(0.8f, 0.1f, 0.1f, 1.0f),

            IconDeletedTint = new Vector4(1.0f, 0.2f, 0.2f, 1.0f),
            IconDefaultTint = new Vector4(1.0f, 1.0f, 1.0f, 1.0f), // Pure white for native colors
            IconDimmedTint = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),

            StatusFallbackOnline = new Vector4(0.2f, 0.7f, 0.2f, 1.0f),
            StatusFallbackOffline = new Vector4(0.6f, 0.6f, 0.6f, 1.0f),
            StatusFallbackDeleted = new Vector4(0.8f, 0.1f, 0.1f, 1.0f),

            WindowBg = new Vector4(0.91f, 0.86f, 0.77f, 0.98f),
            Text = new Vector4(0.15f, 0.11f, 0.07f, 1.0f),
            ChildBg = new Vector4(0.88f, 0.82f, 0.73f, 0.50f),
            PopupBg = new Vector4(0.91f, 0.86f, 0.77f, 0.98f),
            FrameBg = new Vector4(0.85f, 0.78f, 0.65f, 1.0f),
            FrameBgHovered = new Vector4(0.90f, 0.82f, 0.70f, 1.0f),
            FrameBgActive = new Vector4(0.80f, 0.72f, 0.60f, 1.0f),
            TitleBg = new Vector4(0.85f, 0.78f, 0.65f, 1.0f),
            TitleBgActive = new Vector4(0.90f, 0.82f, 0.70f, 1.0f),
            TitleBgCollapsed = new Vector4(0.80f, 0.70f, 0.60f, 1.0f),
            TableHeaderBg = new Vector4(0.82f, 0.73f, 0.60f, 1.0f),
            TableRowBg = new Vector4(0.91f, 0.86f, 0.77f, 1.0f),
            TableRowBgAlt = new Vector4(0.85f, 0.79f, 0.69f, 1.0f),
            Border = new Vector4(0.67f, 0.54f, 0.40f, 1.0f),
            Tab = new Vector4(0.85f, 0.78f, 0.65f, 1.0f),
            TabHovered = new Vector4(0.90f, 0.82f, 0.70f, 1.0f),
            TabActive = new Vector4(0.95f, 0.90f, 0.80f, 1.0f),
            TabUnfocused = new Vector4(0.80f, 0.72f, 0.60f, 1.0f),
            TabUnfocusedActive = new Vector4(0.85f, 0.78f, 0.65f, 1.0f),
            Button = new Vector4(0.85f, 0.78f, 0.65f, 1.0f),
            ButtonHovered = new Vector4(0.90f, 0.82f, 0.70f, 1.0f),
            ButtonActive = new Vector4(0.80f, 0.72f, 0.60f, 1.0f)
        };
    }

    public void SetTheme(ThemeStyle style) {
        if (!this.palettes.ContainsKey(style)) {
            style = ThemeStyle.Dark;
        }

        this.CurrentStyle = style;
        this.CurrentPalette = this.palettes[style];

        var config = this.configurationService.GetConfig();
        if (config.SelectedTheme != (int)style) {
            config.SelectedTheme = (int)style;
            this.configurationService.Save();
        }
    }
}