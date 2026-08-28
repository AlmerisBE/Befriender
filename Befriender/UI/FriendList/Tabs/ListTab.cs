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

public class ListTab : ITab, IDisposable {
    private IFriendRepository friendRepository;
    private FriendListTableComponent tableComponent;
    private FriendProfilePanelComponent profilePanelComponent;
    private ListToolbarComponent toolbarComponent;
    private ILocalizationService loc;
    private IConfigurationService configurationService;
    private RemoveConfirmationModalComponent removeConfirmationModal;

    private bool showOnlineOnly = false;
    private bool groupByGroups = false;
    private bool forceRefresh = false;
    private const float PanelWidth = 300f;
    private FriendProfile? selectedFriend = null;

    public string InternalName => "Tab_List";
    public string Name => this.loc.Translate("Tab_List");
    public bool IsProfilePanelOpen => this.selectedFriend != null;

    public ListTab(IFriendRepository friendRepository, FriendListTableComponent tableComponent, FriendProfilePanelComponent profilePanelComponent, ListToolbarComponent toolbarComponent, ILocalizationService loc, IConfigurationService configurationService, RemoveConfirmationModalComponent removeConfirmationModal) {
        this.friendRepository = friendRepository;
        this.tableComponent = tableComponent;
        this.profilePanelComponent = profilePanelComponent;
        this.toolbarComponent = toolbarComponent;
        this.loc = loc;
        this.configurationService = configurationService;
        this.removeConfirmationModal = removeConfirmationModal;

        this.groupByGroups = this.configurationService.GetConfig().GroupByCustomGroups;
        this.friendRepository.CacheCleared += this.OnCacheCleared;
    }

    private void OnCacheCleared() {
        if (this.selectedFriend != null) {
            this.selectedFriend = null;
            var config = this.configurationService.GetConfig();
            config.IsProfilePanelOpen = false;
            this.configurationService.Save();
        }
    }

    private void ToggleProfilePanel(FriendProfile? friend) {
        var config = this.configurationService.GetConfig();
        if (this.selectedFriend != null && friend != null && this.selectedFriend.ContentId == friend.ContentId) {
            friend = null;
        }

        bool shouldOpen = friend != null;

        if (config.IsProfilePanelOpen != shouldOpen) {
            config.IsProfilePanelOpen = shouldOpen;
            this.configurationService.Save();
        }

        this.selectedFriend = friend;
    }

    public void Draw() {
        var rawFriends = this.friendRepository.GetFriends();
        var activeFriends = rawFriends.Where(f => !f.IsArchived).ToList();

        bool previousGrouping = this.groupByGroups;
        if (this.toolbarComponent.Draw(ref this.showOnlineOnly, ref this.groupByGroups, true)) {
            this.forceRefresh = true;
            if (this.groupByGroups != previousGrouping) {
                var config = this.configurationService.GetConfig();
                config.GroupByCustomGroups = this.groupByGroups;
                this.configurationService.Save();
            }
        }

        ImGui.Separator();
        ImGui.Spacing();

        if (activeFriends.Count == 0) {
            ImGui.Text(this.loc.Translate("List_EmptyOrSyncing"));
            return;
        }

        float tableWidth = this.selectedFriend != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        this.tableComponent.Draw(tableWidth, activeFriends, this.selectedFriend, this.showOnlineOnly, this.groupByGroups, this.forceRefresh, this.ToggleProfilePanel);
        this.forceRefresh = false;

        if (this.selectedFriend != null) {
            ImGui.SameLine();
            this.profilePanelComponent.Draw(PanelWidth, this.selectedFriend, () => this.ToggleProfilePanel(null));
        }

        this.removeConfirmationModal.Draw();
    }

    public void Dispose() {
        this.friendRepository.CacheCleared -= this.OnCacheCleared;
    }
}