namespace Befriender.UI.FriendList.Windows;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Input.Contracts;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Numerics;

public class FriendListWindow : Window, IDisposable {
    private IEnumerable<ITab> tabs;
    private IFriendSyncService syncService;
    private IThemeService themeService;
    private IHotkeyService hotkeyService;

    public FriendListWindow(IEnumerable<ITab> tabs, IFriendSyncService syncService, IThemeService themeService, Befriender.Core.Input.Contracts.IHotkeyService hotkeyService) : base("Befriender", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {
        this.tabs = tabs;
        this.syncService = syncService;
        this.themeService = themeService;
        this.hotkeyService = hotkeyService;

        this.hotkeyService.OnHotkeyPressed += this.Toggle;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(500, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        this.TitleBarButtons.Add(new TitleBarButton {
            Icon = FontAwesomeIcon.Sync,
            IconOffset = new Vector2(1, 1),
            Click = (mouseButton) => this.syncService.RequestServerRefresh()
        });
    }

    public override void PreDraw() {
        var p = this.themeService.CurrentPalette;

        ImGui.PushStyleColor(ImGuiCol.WindowBg, p.WindowBg);
        ImGui.PushStyleColor(ImGuiCol.Text, p.Text);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, p.ChildBg);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, p.PopupBg);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, p.FrameBg);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, p.FrameBgHovered);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, p.FrameBgActive);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, p.TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, p.TitleBgActive);
        ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, p.TitleBgCollapsed);
        ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, p.TableHeaderBg);
        ImGui.PushStyleColor(ImGuiCol.TableRowBg, p.TableRowBg);
        ImGui.PushStyleColor(ImGuiCol.TableRowBgAlt, p.TableRowBgAlt);
        ImGui.PushStyleColor(ImGuiCol.Border, p.Border);
        ImGui.PushStyleColor(ImGuiCol.Tab, p.Tab);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, p.TabHovered);
        ImGui.PushStyleColor(ImGuiCol.TabActive, p.TabActive);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocused, p.TabUnfocused);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, p.TabUnfocusedActive);
        ImGui.PushStyleColor(ImGuiCol.Button, p.Button);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, p.ButtonHovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, p.ButtonActive);
    }

    public override void PostDraw() {
        // Popping the 22 pushed structural colors
        ImGui.PopStyleColor(22);
    }

    public override void Update() {
        if (this.syncService.IsWindowOpen != this.IsOpen) {
            this.syncService.IsWindowOpen = this.IsOpen;

            if (this.IsOpen) {
                this.syncService.RequestServerRefresh();
            }
        }
    }

    public override void Draw() {
        if (ImGui.BeginTabBar("MainTabBar")) {
            foreach (var tab in this.tabs) {
                if (ImGui.BeginTabItem(tab.Name)) {
                    tab.Draw();
                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }

    public void Dispose() {
        this.hotkeyService.OnHotkeyPressed -= this.Toggle;
    }
}