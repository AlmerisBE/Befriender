namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.FriendList.Components;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using System;
using System.Linq;

public class ArchiveTab : ITab, IDisposable {
    private IFriendRepository friendRepository;
    private FriendListTableComponent tableComponent;
    private FriendProfilePanelComponent profilePanelComponent;
    private FriendStatusBarComponent statusBarComponent;
    private ILocalizationService loc;
    private IConfigurationService configurationService;
    private IWindowNavigationService navService;

    private bool showOnlineOnly = false;
    private bool forceRefresh = false;
    private const float PanelWidth = 300f;
    private FriendProfile? selectedFriend = null;

    public string InternalName => "Tab_Archives";
    public string Name => this.loc.Translate("Tab_Archives");

    public ArchiveTab(IFriendRepository friendRepository, FriendListTableComponent tableComponent, FriendProfilePanelComponent profilePanelComponent, FriendStatusBarComponent statusBarComponent, ILocalizationService loc, IConfigurationService configurationService, IWindowNavigationService navService) {
        this.friendRepository = friendRepository;
        this.tableComponent = tableComponent;
        this.profilePanelComponent = profilePanelComponent;
        this.statusBarComponent = statusBarComponent;
        this.loc = loc;
        this.configurationService = configurationService;
        this.navService = navService;

        this.friendRepository.CacheCleared += this.OnCacheCleared;
    }

    private void OnCacheCleared() {
        if (this.selectedFriend != null) {
            this.selectedFriend = null;
            this.navService.ToggleProfilePanel(false);
        }
    }

    private void ToggleProfilePanel(FriendProfile? friend) {
        if (this.selectedFriend != null && friend != null && this.selectedFriend.ContentId == friend.ContentId) {
            friend = null;
        }

        this.navService.ToggleProfilePanel(friend != null);
        this.selectedFriend = friend;
    }

    public void Draw() {
        var rawFriends = this.friendRepository.GetFriends();
        var archivedFriends = rawFriends.Where(f => f.IsArchived).ToList();

        if (archivedFriends.Count == 0) {
            ImGui.Text(this.loc.Translate("Archive_Empty"));
            return;
        }

        float footerHeight = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y;
        float tableWidth = this.selectedFriend != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        this.tableComponent.Draw(tableWidth, footerHeight, archivedFriends, this.selectedFriend, this.showOnlineOnly, this.forceRefresh, this.ToggleProfilePanel);
        this.forceRefresh = false;

        if (this.selectedFriend != null) {
            ImGui.SameLine();
            this.profilePanelComponent.Draw(PanelWidth, footerHeight, this.selectedFriend, () => this.ToggleProfilePanel(null));
        }

        ImGui.Separator();

        if (this.statusBarComponent.Draw(rawFriends, ref this.showOnlineOnly)) {
            this.forceRefresh = true;
        }
    }

    public void Dispose() => this.friendRepository.CacheCleared -= this.OnCacheCleared;
}