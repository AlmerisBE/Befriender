namespace Befriender.UI.Theme.Models;

using System.Numerics;

public class ThemePalette {
    public Vector4 TextOnline { get; set; }
    public Vector4 TextOffline { get; set; }
    public Vector4 TextBusy { get; set; }
    public Vector4 TextArchived { get; set; }
    public Vector4 TextDeleted { get; set; }
    public Vector4 TextMarkedForRemoval { get; set; }

    public Vector4 IconDeletedTint { get; set; }
    public Vector4 IconDefaultTint { get; set; }
    public Vector4 IconDimmedTint { get; set; }

    public Vector4 StatusFallbackOnline { get; set; }
    public Vector4 StatusFallbackOffline { get; set; }
    public Vector4 StatusFallbackDeleted { get; set; }

    public Vector4 WindowBg { get; set; }
    public Vector4 Text { get; set; }
    public Vector4 ChildBg { get; set; }
    public Vector4 PopupBg { get; set; }
    public Vector4 FrameBg { get; set; }
    public Vector4 FrameBgHovered { get; set; }
    public Vector4 FrameBgActive { get; set; }
    public Vector4 TitleBg { get; set; }
    public Vector4 TitleBgActive { get; set; }
    public Vector4 TitleBgCollapsed { get; set; }
    public Vector4 TableHeaderBg { get; set; }
    public Vector4 TableRowBg { get; set; }
    public Vector4 TableRowBgAlt { get; set; }
    public Vector4 Border { get; set; }
    public Vector4 Tab { get; set; }
    public Vector4 TabHovered { get; set; }
    public Vector4 TabActive { get; set; }
    public Vector4 TabUnfocused { get; set; }
    public Vector4 TabUnfocusedActive { get; set; }
    public Vector4 Button { get; set; }
    public Vector4 ButtonHovered { get; set; }
    public Vector4 ButtonActive { get; set; }
}