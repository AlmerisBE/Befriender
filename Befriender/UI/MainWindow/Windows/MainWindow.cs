namespace Befriender.UI.MainWindow.Windows;

using Befriender.Core.Configuration.Contracts;
using Befriender.UI.Input.Contracts;
using Befriender.UI.Localization.Contracts;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.MainWindow.Contracts;
using Befriender.UI.Theme.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class MainWindow : Window, IDisposable {
    private IEnumerable<ITab> tabs = null!;
    private IConfigurationService configurationService = null!;
    private ILocalizationService loc = null!;
    private IWindowNavigationService navService = null!;
    private FriendStatusBarComponent statusBar = null!;
    private RemoveConfirmationModalComponent removeModal = null!;
    private IHotkeyService hotkeyService = null!;
    private IThemeService themeService = null!;

    // Explicitly mark conditionally assigned fields as nullable
    private ITab? currentTab;
    private ITab? tabToFocus;
    private bool wasProfilePanelOpen = false;
    private const float PanelWidth = 300f;

    public MainWindow(
        IEnumerable<ITab> tabs,
        IConfigurationService configurationService,
        ILocalizationService loc,
        IWindowNavigationService navService,
        FriendStatusBarComponent statusBar,
        RemoveConfirmationModalComponent removeModal,
        IHotkeyService hotkeyService,
        IThemeService themeService)
        : base("Befriender", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {

        this.tabs = tabs;
        this.configurationService = configurationService;
        this.loc = loc;
        this.navService = navService;
        this.statusBar = statusBar;
        this.removeModal = removeModal;
        this.hotkeyService = hotkeyService;
        this.themeService = themeService;

        if (this.tabs != null && this.tabs.Any()) {
            this.currentTab = this.tabs.First();
        }

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(400, 300),
            MaximumSize = new Vector2(9999, 9999)
        };

        var config = this.configurationService?.GetConfig();
        this.wasProfilePanelOpen = config?.IsProfilePanelOpen ?? false;

        if (this.navService != null) {
            this.navService.OnTabRequested += this.SetTab;
            this.navService.OnWindowToggleRequested += this.Toggle;
        }

        if (this.hotkeyService != null) {
            this.hotkeyService.OnHotkeyPressed += this.Toggle;
        }
    }

    private void SetTab(string tabInternalName) {
        var tab = this.tabs.FirstOrDefault(t => t.InternalName == tabInternalName);
        if (tab != null) {
            this.tabToFocus = tab;
            this.IsOpen = true;
        }
    }

    public override void PreDraw() {
        var palette = this.themeService.CurrentPalette;

        ImGui.PushStyleColor(ImGuiCol.WindowBg, palette.WindowBg);
        ImGui.PushStyleColor(ImGuiCol.Text, palette.Text);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, palette.ChildBg);
        ImGui.PushStyleColor(ImGuiCol.PopupBg, palette.PopupBg);
        ImGui.PushStyleColor(ImGuiCol.FrameBg, palette.FrameBg);
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered, palette.FrameBgHovered);
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive, palette.FrameBgActive);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, palette.TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, palette.TitleBgActive);
        ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed, palette.TitleBgCollapsed);
        ImGui.PushStyleColor(ImGuiCol.TableHeaderBg, palette.TableHeaderBg);
        ImGui.PushStyleColor(ImGuiCol.TableRowBg, palette.TableRowBg);
        ImGui.PushStyleColor(ImGuiCol.TableRowBgAlt, palette.TableRowBgAlt);
        ImGui.PushStyleColor(ImGuiCol.Border, palette.Border);
        ImGui.PushStyleColor(ImGuiCol.Tab, palette.Tab);
        ImGui.PushStyleColor(ImGuiCol.TabHovered, palette.TabHovered);
        ImGui.PushStyleColor(ImGuiCol.TabActive, palette.TabActive);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocused, palette.TabUnfocused);
        ImGui.PushStyleColor(ImGuiCol.TabUnfocusedActive, palette.TabUnfocusedActive);
        ImGui.PushStyleColor(ImGuiCol.Button, palette.Button);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, palette.ButtonHovered);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, palette.ButtonActive);
    }

    public override void Draw() {
        if (ImGui.BeginTabBar("BefrienderMainTabBar")) {
            foreach (var tab in this.tabs) {
                var flags = this.tabToFocus == tab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;

                if (ImGui.BeginTabItem(tab.Name, flags)) {
                    this.currentTab = tab;
                    tab.Draw();
                    ImGui.EndTabItem();
                }
            }
            this.tabToFocus = null;
            ImGui.EndTabBar();
        }

        ImGui.Spacing();
        ImGui.Separator();
        this.statusBar.Draw();

        this.removeModal.Draw();

        if (this.currentTab != null) {
            bool isPanelOpen = this.currentTab.IsProfilePanelOpen;

            if (isPanelOpen && !this.wasProfilePanelOpen) {
                var size = ImGui.GetWindowSize();
                ImGui.SetWindowSize(new Vector2(size.X + PanelWidth + ImGui.GetStyle().ItemSpacing.X, size.Y));
            }
            else if (!isPanelOpen && this.wasProfilePanelOpen) {
                var size = ImGui.GetWindowSize();
                ImGui.SetWindowSize(new Vector2(Math.Max(this.SizeConstraints?.MinimumSize.X ?? 400, size.X - PanelWidth - ImGui.GetStyle().ItemSpacing.X), size.Y));
            }

            if (this.wasProfilePanelOpen != isPanelOpen) {
                this.wasProfilePanelOpen = isPanelOpen;
                var config = this.configurationService?.GetConfig();
                if (config != null) {
                    config.IsProfilePanelOpen = isPanelOpen;
                    this.configurationService?.Save();
                }
            }
        }
    }

    public override void PostDraw() {
        ImGui.PopStyleColor(22);
    }

    public void Dispose() {
        if (this.navService != null) {
            this.navService.OnTabRequested -= this.SetTab;
            this.navService.OnWindowToggleRequested -= this.Toggle;
        }

        if (this.hotkeyService != null) {
            this.hotkeyService.OnHotkeyPressed -= this.Toggle;
        }
    }
}