namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.FriendList.Components;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using System;
using System.Linq;

public class ArchiveTab : ITab, IDisposable {
    private IFriendRepository friendRepository;
    private ArchiveTableComponent tableComponent;
    private FriendProfilePanelComponent profilePanelComponent;
    private ListToolbarComponent toolbarComponent;
    private ILocalizationService loc;
    private IConfigurationService configurationService;

    private string searchQuery = string.Empty;
    private bool showOnlineOnly = false;
    private bool showNearbyOnly = false;
    private bool groupByGroups = false;
    private const float PanelWidth = 300f;
    private FriendProfile? selectedFriend = null;

    public string InternalName => "Tab_Archives";
    public string Name => this.loc.Translate("Tab_Archives");
    public bool IsProfilePanelOpen => this.selectedFriend != null;

    public ArchiveTab(IFriendRepository friendRepository, ArchiveTableComponent tableComponent, FriendProfilePanelComponent profilePanelComponent, ListToolbarComponent toolbarComponent, ILocalizationService loc, IConfigurationService configurationService) {
        this.friendRepository = friendRepository;
        this.tableComponent = tableComponent;
        this.profilePanelComponent = profilePanelComponent;
        this.toolbarComponent = toolbarComponent;
        this.loc = loc;
        this.configurationService = configurationService;

        this.friendRepository.CacheCleared += this.OnCacheCleared;
    }

    private void OnCacheCleared() {
        this.selectedFriend = null;
    }

    private void ToggleProfilePanel(FriendProfile? friend) {
        if (this.selectedFriend != null && friend != null && this.selectedFriend.ContentId == friend.ContentId) {
            friend = null;
        }

        this.selectedFriend = friend;
    }

    public void Draw() {
        var rawFriends = this.friendRepository.GetFriends();
        var archivedFriends = rawFriends.Where(f => f.IsArchived).ToList();

        var config = this.configurationService.GetConfig();
        this.groupByGroups = config.GroupByCustomGroups;

        bool previousGrouping = this.groupByGroups;
        if (this.toolbarComponent.Draw(ref this.showOnlineOnly, ref this.showNearbyOnly, ref this.groupByGroups, ref this.searchQuery, false)) {
            if (this.groupByGroups != previousGrouping) {
                config.GroupByCustomGroups = this.groupByGroups;
                this.configurationService.Save();
            }
        }

        ImGui.Separator();
        ImGui.Spacing();

        if (archivedFriends.Count == 0) {
            ImGui.Text(this.loc.Translate("Archive_Empty"));
            return;
        }

        float tableWidth = this.selectedFriend != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        this.tableComponent.Draw(tableWidth, archivedFriends, this.selectedFriend, this.showNearbyOnly, this.groupByGroups, this.searchQuery, this.ToggleProfilePanel);

        if (this.selectedFriend != null) {
            ImGui.SameLine();
            this.profilePanelComponent.Draw(PanelWidth, this.selectedFriend, () => this.ToggleProfilePanel(null));
        }
    }

    public void Dispose() => this.friendRepository.CacheCleared -= this.OnCacheCleared;
}