namespace Befriender.UI.FriendList.Windows;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.FriendList.Components;
using Befriender.UI.FriendList.Contracts;
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
        RemoveConfirmationModalComponent removeModal)
        : base("Befriender", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {

        this.tabs = tabs;
        this.configurationService = configurationService;
        this.loc = loc;
        this.navService = navService;
        this.statusBar = statusBar;
        this.removeModal = removeModal;

        this.currentTab = this.tabs.First();

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(400, 300),
            MaximumSize = new Vector2(9999, 9999)
        };

        this.navService.OnTabRequested += this.SetTab;
        this.navService.OnWindowToggleRequested += this.Toggle;
    }

    private void SetTab(string tabInternalName) {
        var tab = this.tabs.FirstOrDefault(t => t.InternalName == tabInternalName);
        if (tab != null) {
            this.tabToFocus = tab;
            this.IsOpen = true;
        }
    }

    public override void Draw() {
        // --- TABS RENDERING ---
        if (ImGui.BeginTabBar("BefrienderMainTabBar")) {
            foreach (var tab in this.tabs) {
                // Apply SetSelected ONLY if an external command requested focus on this specific tab
                var flags = this.tabToFocus == tab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;

                if (ImGui.BeginTabItem(tab.Name, flags)) {
                    this.currentTab = tab;
                    tab.Draw();
                    ImGui.EndTabItem();
                }
            }
            // Reset focus request to allow natural ImGui navigation afterward
            this.tabToFocus = null;
            ImGui.EndTabBar();
        }

        // --- FOOTER RENDERING ---
        ImGui.Spacing();
        ImGui.Separator();
        this.statusBar.Draw();

        // --- GLOBAL MODALS ---
        this.removeModal.Draw();

        // --- DYNAMIC RESIZING ---
        if (this.currentTab != null) {
            bool isPanelOpen = this.currentTab.IsProfilePanelOpen;

            if (isPanelOpen && !this.wasProfilePanelOpen) {
                // Expand window to accommodate the side panel
                var size = ImGui.GetWindowSize();
                ImGui.SetWindowSize(new Vector2(size.X + PanelWidth + ImGui.GetStyle().ItemSpacing.X, size.Y));
            }
            else if (!isPanelOpen && this.wasProfilePanelOpen) {
                // Shrink window back to normal size
                var size = ImGui.GetWindowSize();
                ImGui.SetWindowSize(new Vector2(Math.Max(this.SizeConstraints?.MinimumSize.X ?? 400, size.X - PanelWidth - ImGui.GetStyle().ItemSpacing.X), size.Y));
            }

            this.wasProfilePanelOpen = isPanelOpen;
        }
    }

    public void Dispose() {
        this.navService.OnTabRequested -= this.SetTab;
        this.navService.OnWindowToggleRequested -= this.Toggle;
    }
}