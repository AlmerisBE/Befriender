namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Numerics;

public class ListTab : ITab {
    private IFriendRepository friendRepository;
    private IFriendDisplayService displayService;
    private IFriendSyncService syncService;
    private IGameDataService gameDataService;
    private ITextureProvider textureProvider;
    private DateTime lastProcessedSyncTime = DateTime.MinValue;

    private bool showOnlineOnly = false;
    private IReadOnlyList<FriendProfile> cachedFriends = new List<FriendProfile>();
    private int lastFriendCount = -1;
    private bool forceRefresh = false;

    // Profile Panel State
    private FriendProfile? selectedFriend = null;
    private string notesBuffer = string.Empty;

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

        float footerHeight = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y;

        // Calculate dynamic width to accommodate the retractable profile panel
        float panelWidth = 300f;
        float tableWidth = this.selectedFriend != null ? ImGui.GetContentRegionAvail().X - panelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        if (ImGui.BeginTable("FriendsTable", 8, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable, new Vector2(tableWidth, -footerHeight))) {
            ImGui.TableSetupColumn("Status", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("FC", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Location");
            ImGui.TableSetupColumn("Added", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Last Seen", ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            this.HandleSortingAndFiltering(rawFriends);

            float textOffsetY = Math.Max(0, (24.0f - ImGui.GetTextLineHeight()) * 0.5f);

            foreach (var friend in this.cachedFriends) {
                ImGui.TableNextRow();

                bool isAvailable = this.gameDataService.IsFriendAvailable(friend.OnlineStateMask);
                Vector4 rowColor;

                if (!friend.IsOnline) {
                    rowColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                }
                else if (!isAvailable) {
                    rowColor = new Vector4(0.75f, 0.75f, 0.75f, 1.0f);
                }
                else {
                    rowColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                }

                ImGui.PushStyleColor(ImGuiCol.Text, rowColor);

                // 1. Status Column
                ImGui.TableNextColumn();
                float statusColWidth = ImGui.GetColumnWidth();

                // Stealthy row selection spanning all columns
                var cursorStart = ImGui.GetCursorPos();
                bool isSelected = this.selectedFriend == friend;
                if (ImGui.Selectable($"##row_{friend.ContentId}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) {
                    this.selectedFriend = friend;
                    this.notesBuffer = friend.Notes ?? string.Empty;
                }
                ImGui.SetCursorPos(cursorStart); // Reset cursor to draw the icon over the selectable properly

                ulong effectiveMask = friend.IsOnline ? friend.OnlineStateMask : 0;
                var statusInfo = this.gameDataService.GetOnlineStatusInfo(effectiveMask);

                var iconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = statusInfo.IconId };
                var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

                if (iconWrap != null) {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - 24.0f) * 0.5f));
                    ImGui.Image(iconWrap.Handle, new Vector2(24, 24));
                    if (ImGui.IsItemHovered()) {
                        ImGui.SetTooltip(statusInfo.Name);
                    }
                }
                else {
                    float textWidth = ImGui.CalcTextSize("●").X;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - textWidth) * 0.5f));
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                    Vector4 fallbackColor = friend.IsOnline ? new Vector4(0.43f, 0.85f, 0.43f, 1.0f) : new Vector4(0.5f, 0.5f, 0.5f, 1.0f);

                    ImGui.PushStyleColor(ImGuiCol.Text, fallbackColor);
                    ImGui.Text("●");
                    ImGui.PopStyleColor();

                    if (ImGui.IsItemHovered()) {
                        ImGui.SetTooltip(statusInfo.Name);
                    }
                }

                // 2. Name Column
                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                ImGui.Text(friend.Name);

                // 3. Job Column
                ImGui.TableNextColumn();
                float jobColWidth = ImGui.GetColumnWidth();

                if (friend.JobId > 0) {
                    var iconId = this.gameDataService.GetJobIconId(friend.JobId);
                    var jobAbbr = this.gameDataService.GetJobAbbreviation(friend.JobId);
                    bool iconDrawn = false;

                    if (iconId > 0) {
                        iconLookup = new GameIconLookup { IconId = iconId };
                        iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

                        if (iconWrap != null) {
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (jobColWidth - 24.0f) * 0.5f));
                            var imageTint = friend.IsOnline ? rowColor : new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                            ImGui.Image(iconWrap.Handle, new Vector2(24, 24), Vector2.Zero, Vector2.One, imageTint);
                            if (ImGui.IsItemHovered()) {
                                ImGui.SetTooltip(jobAbbr);
                            }

                            iconDrawn = true;
                        }
                    }

                    if (!iconDrawn) {
                        float textWidth = ImGui.CalcTextSize(jobAbbr).X;
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (jobColWidth - textWidth) * 0.5f));
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                        ImGui.Text(jobAbbr);
                    }
                }
                else {
                    ImGui.Text(string.Empty);
                }

                // 4. FC Column
                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                if (!string.IsNullOrEmpty(friend.FcTag)) {
                    ImGui.Text($"<{friend.FcTag}>");
                }
                else {
                    ImGui.Text(string.Empty);
                }

                // 5. World Column
                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                ImGui.Text(this.gameDataService.GetWorldName(friend.HomeWorldId));

                // 6. Location Column
                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                var locationName = this.gameDataService.GetLocationName(friend.LocationId);
                ImGui.Text(string.IsNullOrEmpty(locationName) || locationName == "0" ? "Unknown" : locationName);

                // 7. Added Column
                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                var dateStr = friend.AddedAt == DateTime.MinValue ? "Unknown" : friend.AddedAt.ToShortDateString();
                var locStr = this.gameDataService.GetLocationName(friend.AddedLocationId);
                ImGui.Text($"{dateStr} ({locStr})");

                // 8. Last Seen Column
                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                if (friend.IsOnline) {
                    ImGui.Text("Online");
                }
                else if (friend.LastSeenAt == DateTime.MinValue) {
                    ImGui.Text("Unknown");
                }
                else {
                    var diff = DateTime.Now - friend.LastSeenAt;
                    if (diff.TotalDays >= 1) {
                        ImGui.Text($"{(int)diff.TotalDays}d");
                    }
                    else if (diff.TotalHours >= 1) {
                        ImGui.Text($"{(int)diff.TotalHours}h");
                    }
                    else if (diff.TotalMinutes >= 1) {
                        ImGui.Text($"{(int)diff.TotalMinutes}m");
                    }
                    else {
                        ImGui.Text("Just now");
                    }
                }

                ImGui.PopStyleColor(); // Remove row color
            }

            ImGui.EndTable();
        }

        // Draw Profile Panel if a friend is selected
        if (this.selectedFriend != null) {
            ImGui.SameLine();
            if (ImGui.BeginChild("ProfilePanel", new Vector2(panelWidth, -footerHeight), true)) {
                this.DrawProfilePanel();
            }
            ImGui.EndChild();
        }

        ImGui.Separator();

        if (ImGui.Checkbox("Show Online Only", ref this.showOnlineOnly)) {
            this.forceRefresh = true;
        }

        ImGui.SameLine();

        int onlineCount = 0;
        foreach (var f in rawFriends) {
            if (f.IsOnline) {
                onlineCount++;
            }
        }

        string syncText;
        if (this.syncService.IsSyncPending || this.syncService.LastSyncTime == DateTime.MinValue) {
            syncText = "Scanning...";
        }
        else {
            var diff = DateTime.Now - this.syncService.LastSyncTime;
            string timeStr;
            if (diff.TotalDays >= 1) {
                timeStr = $"{(int)diff.TotalDays}d ago";
            }
            else if (diff.TotalHours >= 1) {
                timeStr = $"{(int)diff.TotalHours}h ago";
            }
            else if (diff.TotalMinutes >= 1) {
                timeStr = $"{(int)diff.TotalMinutes}m ago";
            }
            else {
                timeStr = "Just now";
            }

            syncText = $"Last Sync: {timeStr}";
        }

        var statusText = $"{syncText} | Online: {onlineCount} / Total: {rawFriends.Count}";

        var textSize = ImGui.CalcTextSize(statusText);
        var rightAlignPos = ImGui.GetWindowWidth() - textSize.X - (ImGui.GetStyle().WindowPadding.X * 2) - 30.0f;

        ImGui.SetCursorPosX(Math.Max(rightAlignPos, ImGui.GetCursorPosX()));
        ImGui.Text(statusText);
    }

    private void DrawProfilePanel() {
        var friend = this.selectedFriend!;

        // Header
        ImGui.TextUnformatted(friend.Name);
        ImGui.SameLine(ImGui.GetContentRegionAvail().X - 20);
        if (ImGui.Button("X")) {
            this.selectedFriend = null;
            return;
        }

        ImGui.Separator();
        ImGui.Spacing();

        // Details
        var jobAbbr = friend.JobId > 0 ? this.gameDataService.GetJobAbbreviation(friend.JobId) : "None";
        ImGui.Text($"Job: {jobAbbr}");
        ImGui.Text($"World: {this.gameDataService.GetWorldName(friend.HomeWorldId)}");
        if (!string.IsNullOrEmpty(friend.FcTag)) {
            ImGui.Text($"Free Company: <{friend.FcTag}>");
        }

        ImGui.Spacing();
        ImGui.Text("--- Metadata ---");
        var dateStr = friend.AddedAt == DateTime.MinValue ? "Unknown" : friend.AddedAt.ToShortDateString();
        var locStr = this.gameDataService.GetLocationName(friend.AddedLocationId);
        ImGui.Text($"Added: {dateStr}");
        ImGui.Text($"Met at: {locStr}");

        string lastSeenStr;
        if (friend.IsOnline) {
            lastSeenStr = "Online";
        }
        else if (friend.LastSeenAt == DateTime.MinValue) {
            lastSeenStr = "Unknown";
        }
        else {
            var diff = DateTime.Now - friend.LastSeenAt;
            if (diff.TotalDays >= 1) {
                lastSeenStr = $"{(int)diff.TotalDays} days ago";
            }
            else if (diff.TotalHours >= 1) {
                lastSeenStr = $"{(int)diff.TotalHours} hours ago";
            }
            else if (diff.TotalMinutes >= 1) {
                lastSeenStr = $"{(int)diff.TotalMinutes} mins ago";
            }
            else {
                lastSeenStr = "Just now";
            }
        }
        ImGui.Text($"Last Seen: {lastSeenStr}");

        ImGui.Spacing();
        ImGui.Text($"List Status: {(friend.IsArchived ? "Archived" : "Active")}");

        ImGui.Spacing();
        ImGui.Text("--- Notes ---");
        if (ImGui.InputTextMultiline("##notes", ref this.notesBuffer, 2048, new Vector2(-1, 100))) {
            // Edit in progress
        }
        if (ImGui.IsItemDeactivatedAfterEdit()) {
            friend.Notes = this.notesBuffer;
            this.friendRepository.Save();
        }

        // Name History (US-2.2)
        if (friend.PreviousNames != null && friend.PreviousNames.Count > 0) {
            ImGui.Spacing();
            ImGui.Text("--- Name History ---");
            foreach (var oldName in friend.PreviousNames) {
                ImGui.BulletText(oldName);
            }
        }

        ImGui.Spacing();
        ImGui.Separator();

        // Archive / Restore Actions (Prep for US-3.3 / US-3.4)
        if (friend.IsArchived) {
            if (ImGui.Button("Restore Friend", new Vector2(-1, 0))) {
                friend.IsArchived = false;
                this.friendRepository.Save();
            }
        }
        else {
            if (ImGui.Button("Archive Friend", new Vector2(-1, 0))) {
                friend.IsArchived = true;
                this.friendRepository.Save();
            }
        }
    }

    private void HandleSortingAndFiltering(IReadOnlyList<FriendProfile> rawFriends) {
        var sortSpecs = ImGui.TableGetSortSpecs();
        bool dataUpdated = this.syncService.LastSyncTime != this.lastProcessedSyncTime;

        if (sortSpecs.SpecsDirty || this.forceRefresh || rawFriends.Count != this.lastFriendCount || dataUpdated) {
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
            this.lastProcessedSyncTime = this.syncService.LastSyncTime;
        }
    }
}