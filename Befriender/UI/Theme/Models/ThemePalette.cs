namespace Befriender.UI.Theme.Models;

using System.Numerics;

public class ThemePalette {
    public Vector4 TextOnline { get; set; }
    public Vector4 TextOffline { get; set; }
    public Vector4 TextBusy { get; set; }
    public Vector4 TextArchived { get; set; }
    public Vector4 TextDeleted { get; set; }

    public Vector4 IconDeletedTint { get; set; }
    public Vector4 IconDefaultTint { get; set; }
    public Vector4 IconOfflineTint { get; set; }

    public Vector4 StatusFallbackOnline { get; set; }
    public Vector4 StatusFallbackOffline { get; set; }
    public Vector4 StatusFallbackDeleted { get; set; }
}