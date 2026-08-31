namespace Befriender.UI.MainWindow.Windows;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.Input.Contracts;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.MainWindow.Contracts;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class MainWindow : Window, IDisposable {
    private IEnumerable<ITab> tabs;
    private IConfigurationService configurationService;
    private ILocalizationService loc;
    private IWindowNavigationService navService;
    private FriendStatusBarComponent statusBar;
    private RemoveConfirmationModalComponent removeModal;
    private IHotkeyService hotkeyService;
    private IThemeService themeService;

    private ITab currentTab;
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

        this.currentTab = this.tabs.First();

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(400, 300),
            MaximumSize = new Vector2(9999, 9999)
        };

        this.navService.OnTabRequested += this.SetTab;
        this.navService.OnWindowToggleRequested += this.Toggle;
        this.hotkeyService.OnHotkeyPressed += this.Toggle;
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

            this.wasProfilePanelOpen = isPanelOpen;
        }
    }

    public override void PostDraw() {
        // Pop exact same amount of pushed style colors (22)
        ImGui.PopStyleColor(22);
    }

    public void Dispose() {
        this.navService.OnTabRequested -= this.SetTab;
        this.navService.OnWindowToggleRequested -= this.Toggle;
        this.hotkeyService.OnHotkeyPressed -= this.Toggle;
    }
}