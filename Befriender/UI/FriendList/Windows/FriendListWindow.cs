namespace Befriender.UI.FriendList.Windows;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Localization.Contracts;
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

    private ITab currentTab;

    public FriendListWindow(
        IEnumerable<ITab> tabs,
        IConfigurationService configurationService,
        ILocalizationService loc,
        IWindowNavigationService navService)
        : base("Befriender", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {

        this.tabs = tabs;
        this.configurationService = configurationService;
        this.loc = loc;
        this.navService = navService;

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
            this.currentTab = tab;
            this.IsOpen = true;
        }
    }

    public override void Draw() {
        if (Dalamud.Bindings.ImGui.ImGui.BeginTabBar("BefrienderMainTabBar")) {
            foreach (var tab in this.tabs) {
                var flags = this.currentTab == tab ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None;

                if (Dalamud.Bindings.ImGui.ImGui.BeginTabItem(tab.Name, flags)) {
                    this.currentTab = tab;
                    tab.Draw();
                    Dalamud.Bindings.ImGui.ImGui.EndTabItem();
                }
            }
            Dalamud.Bindings.ImGui.ImGui.EndTabBar();
        }
    }

    public void Dispose() {
        this.navService.OnTabRequested -= this.SetTab;
        this.navService.OnWindowToggleRequested -= this.Toggle;
    }
}