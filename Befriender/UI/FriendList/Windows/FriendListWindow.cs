namespace Befriender.UI.FriendList.Windows;

using Befriender.Core.Friends.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using System.Collections.Generic;
using System.Numerics;

public class FriendListWindow : Window {
    private IEnumerable<ITab> tabs;
    private IFriendSyncService syncService;

    public FriendListWindow(IEnumerable<ITab> tabs, IFriendSyncService syncService) : base("Befriender", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {
        this.tabs = tabs;
        this.syncService = syncService;

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

    public override void Update() {
        // Dalamud's state restoration bypasses OnOpen(). 
        // We monitor the state explicitly every frame to ensure perfect synchronization.
        if (this.syncService.IsWindowOpen != this.IsOpen) {
            this.syncService.IsWindowOpen = this.IsOpen;

            if (this.IsOpen) {
                // Mimic the opening behavior reliably
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
}