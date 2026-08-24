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

        // Calculate footer height + padding to constrain the table size natively
        float footerHeight = ImGui.GetFrameHeightWithSpacing() + ImGui.GetStyle().ItemSpacing.Y;

        // By passing the size directly to BeginTable, ImGui handles the ScrollY internally and LOCKS the headers
        if (ImGui.BeginTable("FriendsTable", 8, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable, new Vector2(0, -footerHeight))) {
            ImGui.TableSetupColumn("Status", ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Job", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("FC", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("World", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Location");
            ImGui.TableSetupColumn("Added", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Last Seen", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            this.HandleSortingAndFiltering(rawFriends);

            // Pre-calculate the Y offset to perfectly center text against 24x24 icons
            float textOffsetY = Math.Max(0, (24.0f - ImGui.GetTextLineHeight()) * 0.5f);

            foreach (var friend in this.cachedFriends) {
                ImGui.TableNextRow();

                bool isAvailable = this.gameDataService.IsFriendAvailable(friend.OnlineStateMask);
                Vector4 rowColor;

                // Determine row brightness based on availability and online status
                if (!friend.IsOnline) {
                    rowColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                }
                else if (!isAvailable) {
                    rowColor = new Vector4(0.75f, 0.75f, 0.75f, 1.0f); // Dimmed
                }
                else {
                    rowColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f); // Pure white
                }

                ImGui.PushStyleColor(ImGuiCol.Text, rowColor);

                // 1. Status Column (Centered, 24x24 Icon)
                ImGui.TableNextColumn();
                float statusColWidth = ImGui.GetColumnWidth();

                if (!friend.IsOnline) {
                    float textWidth = ImGui.CalcTextSize("●").X;
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - textWidth) * 0.5f));
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                    ImGui.Text("●");

                    if (ImGui.IsItemHovered()) {
                        ImGui.SetTooltip("Offline");
                    }
                }
                else {
                    var statusInfo = this.gameDataService.GetOnlineStatusInfo(friend.OnlineStateMask);
                    bool statusIconDrawn = false;

                    if (statusInfo.IconId > 0 && statusInfo.IconId != 61505) {
                        var iconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = statusInfo.IconId };
                        var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

                        if (iconWrap != null) {
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - 24.0f) * 0.5f));
                            ImGui.Image(iconWrap.Handle, new Vector2(24, 24));

                            if (ImGui.IsItemHovered()) {
                                ImGui.SetTooltip(statusInfo.Name);
                            }

                            statusIconDrawn = true;
                        }
                    }

                    if (!statusIconDrawn) {
                        float textWidth = ImGui.CalcTextSize("●").X;
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - textWidth) * 0.5f));
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);

                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.43f, 0.85f, 0.43f, 1.0f));
                        ImGui.Text("●");
                        ImGui.PopStyleColor();

                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(statusInfo.Name);
                        }
                    }
                }

                // 2. Name Column (Vertically centered text)
                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                ImGui.Text(friend.Name);

                // 3. Job Column (Centered, 24x24 Icon)
                ImGui.TableNextColumn();
                float jobColWidth = ImGui.GetColumnWidth();

                if (friend.JobId > 0) {
                    var iconId = this.gameDataService.GetJobIconId(friend.JobId);
                    var jobAbbr = this.gameDataService.GetJobAbbreviation(friend.JobId);
                    bool iconDrawn = false;

                    if (iconId > 0) {
                        var iconLookup = new GameIconLookup { IconId = iconId };
                        var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

                        if (iconWrap != null) {
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (jobColWidth - 24.0f) * 0.5f));

                            // Apply dimmed tint to job icon if character is unavailable/offline
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

        // Add 30.0f extra margin to ensure the text stays well clear of the resize grip
        var textSize = ImGui.CalcTextSize(statusText);
        var rightAlignPos = ImGui.GetWindowWidth() - textSize.X - (ImGui.GetStyle().WindowPadding.X * 2) - 30.0f;

        ImGui.SetCursorPosX(Math.Max(rightAlignPos, ImGui.GetCursorPosX()));
        ImGui.Text(statusText);
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