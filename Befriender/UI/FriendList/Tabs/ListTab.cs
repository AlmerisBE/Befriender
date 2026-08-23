namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Numerics;

public class ListTab : ITab {
    private IFriendRepository friendRepository;
    private IFriendDisplayService displayService;
    private IFriendSyncService syncService;
    private IGameDataService gameDataService;
    private ITextureProvider textureProvider;

    private bool showOnlineOnly = false;
    private IReadOnlyList<FriendProfile> cachedFriends = new List<FriendProfile>();
    private int lastFriendCount = -1;
    private bool forceRefresh = false;

    public string Name => "List";

    public ListTab(IFriendRepository friendRepository, IFriendDisplayService displayService, IFriendSyncService syncService, IGameDataService gameDataService, ITextureProvider textureProvider) {
        this.friendRepository = friendRepository;
        this.displayService = displayService;
        this.syncService = syncService;
        this.gameDataService = gameDataService;
        this.textureProvider = textureProvider;
    }

    public void Draw() {
        var rawFriends = this.friendRepository.GetFriends();

        if (rawFriends.Count == 0) {
            ImGui.Text("Your friend list is currently empty or syncing...");
            return;
        }

        float footerHeight = ImGui.GetFrameHeightWithSpacing();

        if (ImGui.BeginChild("TableChild", new Vector2(0, -footerHeight), false)) {
            if (ImGui.BeginTable("FriendsTable", 6, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable)) {
                ImGui.TableSetupColumn("Status", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Name");
                ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("FC", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Added", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableHeadersRow();

                this.HandleSortingAndFiltering(rawFriends);

                foreach (var friend in this.cachedFriends) {
                    ImGui.TableNextRow();

                    ImGui.TableNextColumn();
                    ImGui.Text(friend.IsOnline ? "Online" : "Offline");

                    ImGui.TableNextColumn();
                    ImGui.Text(friend.Name);

                    ImGui.TableNextColumn();
                    if (friend.JobId > 0) {
                        var iconId = this.gameDataService.GetJobIconId(friend.JobId);
                        var jobAbbr = this.gameDataService.GetJobAbbreviation(friend.JobId);
                        bool iconDrawn = false;

                        if (iconId > 0) {
                            var iconLookup = new GameIconLookup { IconId = iconId };
                            var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

                            // If texture is loaded, render it
                            if (iconWrap != null) {
                                // 24x24 is the standard comfortable icon size for Dalamud lists
                                var iconSize = new Vector2(24, 24);
                                ImGui.Image(iconWrap.Handle, iconSize);

                                // Show job abbreviation in tooltip on hover
                                if (ImGui.IsItemHovered()) {
                                    ImGui.SetTooltip(jobAbbr);
                                }
                                iconDrawn = true;
                            }
                        }

                        // Fallback text while texture is loading asynchronously
                        if (!iconDrawn) {
                            ImGui.Text(jobAbbr);
                        }
                    }
                    else {
                        ImGui.Text(string.Empty);
                    }

                    ImGui.TableNextColumn();
                    if (!string.IsNullOrEmpty(friend.FcTag)) {
                        ImGui.Text($"<{friend.FcTag}>");
                    }
                    else {
                        ImGui.Text(string.Empty);
                    }

                    ImGui.TableNextColumn();
                    ImGui.Text(this.gameDataService.GetWorldName(friend.HomeWorldId));

                    ImGui.TableNextColumn();
                    var dateStr = friend.AddedAt == System.DateTime.MinValue ? "Unknown" : friend.AddedAt.ToShortDateString();
                    var locStr = this.gameDataService.GetLocationName(friend.AddedLocationId);
                    ImGui.Text($"{dateStr} ({locStr})");
                }

                ImGui.EndTable();
            }
            ImGui.EndChild();
        }

        ImGui.Separator();
        if (ImGui.Checkbox("Show Online Only", ref this.showOnlineOnly)) {
            this.forceRefresh = true;
        }

        ImGui.SameLine();
        var syncText = this.syncService.LastSyncTime == System.DateTime.MinValue
            ? "Syncing..."
            : $"Last Sync: {this.syncService.LastSyncTime.ToShortTimeString()}";

        ImGui.Text($"| {syncText} | Total: {rawFriends.Count}");
    }

    private void HandleSortingAndFiltering(IReadOnlyList<FriendProfile> rawFriends) {
        var sortSpecs = ImGui.TableGetSortSpecs();

        if (sortSpecs.SpecsDirty || this.forceRefresh || rawFriends.Count != this.lastFriendCount) {
            int sortColumn = -1;
            bool isAscending = true;

            if (sortSpecs.SpecsCount > 0) {
                var spec = sortSpecs.Specs[0];
                sortColumn = spec.ColumnIndex;
                isAscending = spec.SortDirection == ImGuiSortDirection.Ascending;
            }

            this.cachedFriends = this.displayService.ProcessFriends(rawFriends, this.showOnlineOnly, sortColumn, isAscending);

            sortSpecs.SpecsDirty = false;
            this.forceRefresh = false;
            this.lastFriendCount = rawFriends.Count;
        }
    }
}