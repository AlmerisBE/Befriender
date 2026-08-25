namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.FriendList.Components;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

public class ListTab : ITab {
    private IFriendRepository friendRepository;
    private FriendListTableComponent tableComponent;
    private FriendProfilePanelComponent profilePanelComponent;
    private FriendStatusBarComponent statusBarComponent;
    private ILocalizationService loc;

    private bool showOnlineOnly = false;
    private bool forceRefresh = false;
    private const float PanelWidth = 300f;
    private FriendProfile? selectedFriend = null;
    private float pendingWidthDelta = 0f;

    public string Name => this.loc.Translate("Tab_List");

    public ListTab(IFriendRepository friendRepository, FriendListTableComponent tableComponent, FriendProfilePanelComponent profilePanelComponent, FriendStatusBarComponent statusBarComponent, ILocalizationService loc) {
        this.friendRepository = friendRepository;
        this.tableComponent = tableComponent;
        this.profilePanelComponent = profilePanelComponent;
        this.statusBarComponent = statusBarComponent;
        this.loc = loc;
    }

    private void ToggleProfilePanel(FriendProfile? friend) {
        if (this.selectedFriend == null && friend != null) {
            this.pendingWidthDelta = PanelWidth;
        }
        else if (this.selectedFriend != null && friend == null) {
            this.pendingWidthDelta = -PanelWidth;
        }

        this.selectedFriend = friend;
    }

    public void Draw() {
        var rawFriends = this.friendRepository.GetFriends();

        if (rawFriends.Count == 0) {
            ImGui.Text(this.loc.Translate("List_EmptyOrSyncing"));
            return;
        }

        float footerHeight = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y;
        float tableWidth = this.selectedFriend != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        this.tableComponent.Draw(tableWidth, footerHeight, rawFriends, this.selectedFriend, this.showOnlineOnly, this.forceRefresh, this.ToggleProfilePanel);
        this.forceRefresh = false;

        if (this.selectedFriend != null) {
            ImGui.SameLine();
            this.profilePanelComponent.Draw(PanelWidth, footerHeight, this.selectedFriend, () => this.ToggleProfilePanel(null));
        }

        ImGui.Separator();

        bool refreshRequested = this.statusBarComponent.Draw(rawFriends, ref this.showOnlineOnly);
        if (refreshRequested) {
            this.forceRefresh = true;
        }

        if (this.pendingWidthDelta != 0f) {
            var currentSize = ImGui.GetWindowSize();
            ImGui.SetWindowSize(new Vector2(Math.Max(500f, currentSize.X + this.pendingWidthDelta), currentSize.Y));
            this.pendingWidthDelta = 0f;
        }
    }
}