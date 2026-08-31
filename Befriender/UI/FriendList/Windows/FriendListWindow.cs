namespace Befriender.UI.FriendList.Windows;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.FriendList.Components;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.Input.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class FriendListWindow : Window, IDisposable {
    private IEnumerable<ITab> tabs;
    private IConfigurationService configurationService;
    private ILocalizationService loc;
    private IWindowNavigationService navService;
    private FriendStatusBarComponent statusBar;
    private RemoveConfirmationModalComponent removeModal;
    private IHotkeyService hotkeyService;

    private ITab currentTab;
    private ITab? tabToFocus;
    private bool wasProfilePanelOpen = false;
    private const float PanelWidth = 300f;

    public FriendListWindow(
        IEnumerable<ITab> tabs,
        IConfigurationService configurationService,
        ILocalizationService loc,
        IWindowNavigationService navService,
        FriendStatusBarComponent statusBar,
        RemoveConfirmationModalComponent removeModal,
        IHotkeyService hotkeyService)
        : base("Befriender", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {

        this.tabs = tabs;
        this.configurationService = configurationService;
        this.loc = loc;
        this.navService = navService;
        this.statusBar = statusBar;
        this.removeModal = removeModal;
        this.hotkeyService = hotkeyService;

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

    public void Dispose() {
        this.navService.OnTabRequested -= this.SetTab;
        this.navService.OnWindowToggleRequested -= this.Toggle;
        this.hotkeyService.OnHotkeyPressed -= this.Toggle;
    }
}