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
using System.Numerics;

public class ListTab : ITab, IDisposable {
    private IFriendRepository friendRepository;
    private FriendListTableComponent tableComponent;
    private FriendProfilePanelComponent profilePanelComponent;
    private FriendStatusBarComponent statusBarComponent;
    private ILocalizationService loc;
    private IConfigurationService configurationService;

    private bool showOnlineOnly = false;
    private bool forceRefresh = false;
    private const float PanelWidth = 300f;
    private FriendProfile? selectedFriend = null;
    private float pendingWidthDelta = 0f;
    private bool isFirstFrame = true;

    public string Name => this.loc.Translate("Tab_List");

    public ListTab(IFriendRepository friendRepository, FriendListTableComponent tableComponent, FriendProfilePanelComponent profilePanelComponent, FriendStatusBarComponent statusBarComponent, ILocalizationService loc, IConfigurationService configurationService) {
        this.friendRepository = friendRepository;
        this.tableComponent = tableComponent;
        this.profilePanelComponent = profilePanelComponent;
        this.statusBarComponent = statusBarComponent;
        this.loc = loc;
        this.configurationService = configurationService;

        this.friendRepository.CacheCleared += this.OnCacheCleared;
    }

    private void OnCacheCleared() {
        if (this.selectedFriend != null) {
            this.selectedFriend = null;
            this.pendingWidthDelta = -PanelWidth;

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

        if (this.selectedFriend == null && friend != null) {
            this.pendingWidthDelta = PanelWidth;
            config.IsProfilePanelOpen = true;
            this.configurationService.Save();
        }
        else if (this.selectedFriend != null && friend == null) {
            this.pendingWidthDelta = -PanelWidth;
            config.IsProfilePanelOpen = false;
            this.configurationService.Save();
        }

        this.selectedFriend = friend;
    }

    public void Draw() {
        if (this.isFirstFrame) {
            this.isFirstFrame = false;
            var config = this.configurationService.GetConfig();

            if (config.IsProfilePanelOpen) {
                this.pendingWidthDelta = -PanelWidth;
                config.IsProfilePanelOpen = false;
                this.configurationService.Save();
            }
        }

        var rawFriends = this.friendRepository.GetFriends();

        // We only hide archived friends from the main list.
        // Deleted characters that are still taking a vanilla slot MUST be shown.
        var activeFriends = rawFriends.Where(f => !f.IsArchived).ToList();

        if (activeFriends.Count == 0) {
            ImGui.Text(this.loc.Translate("List_EmptyOrSyncing"));
            return;
        }

        float footerHeight = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y;
        float tableWidth = this.selectedFriend != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        this.tableComponent.Draw(tableWidth, footerHeight, activeFriends, this.selectedFriend, this.showOnlineOnly, this.forceRefresh, this.ToggleProfilePanel);
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

    public void Dispose() {
        this.friendRepository.CacheCleared -= this.OnCacheCleared;
    }
}