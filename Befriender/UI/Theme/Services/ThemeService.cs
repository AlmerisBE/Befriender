namespace Befriender.UI.Theme.Services;

using Befriender.UI.Theme.Contracts;
using Befriender.UI.Theme.Models;
using System.Numerics;

public class ThemeService : IThemeService {
    public ThemePalette CurrentPalette { get; private set; }

    public ThemeService() {
        this.CurrentPalette = new ThemePalette {
            TextOnline = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            TextOffline = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
            TextBusy = new Vector4(0.75f, 0.75f, 0.75f, 1.0f),
            TextArchived = new Vector4(0.45f, 0.45f, 0.6f, 1.0f),
            TextDeleted = new Vector4(0.8f, 0.4f, 0.4f, 1.0f),

            IconDeletedTint = new Vector4(1.0f, 0.2f, 0.2f, 1.0f),
            IconDefaultTint = new Vector4(1.0f, 1.0f, 1.0f, 1.0f),
            IconOfflineTint = new Vector4(0.8f, 0.2f, 0.2f, 1.0f), // Specifically for the "X" or red offline mark

            StatusFallbackOnline = new Vector4(0.43f, 0.85f, 0.43f, 1.0f),
            StatusFallbackOffline = new Vector4(0.5f, 0.5f, 0.5f, 1.0f),
            StatusFallbackDeleted = new Vector4(0.8f, 0.2f, 0.2f, 1.0f)
        };
    }
}