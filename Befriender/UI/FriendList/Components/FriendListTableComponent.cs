namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.FriendList.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Numerics;

public class FriendListTableComponent {
    private IFriendDisplayService displayService;
    private IFriendSyncService syncService;
    private IGameDataService gameDataService;
    private ITextureProvider textureProvider;
    private ILocalizationService loc;

    private IReadOnlyList<FriendProfile> cachedFriends = new List<FriendProfile>();
    private int lastFriendCount = -1;
    private DateTime lastProcessedSyncTime = DateTime.MinValue;

    public FriendListTableComponent(IFriendDisplayService displayService, IFriendSyncService syncService, IGameDataService gameDataService, ITextureProvider textureProvider, ILocalizationService loc) {
        this.displayService = displayService;
        this.syncService = syncService;
        this.gameDataService = gameDataService;
        this.textureProvider = textureProvider;
        this.loc = loc;
    }

    public void Draw(float tableWidth, float footerHeight, IReadOnlyList<FriendProfile> rawFriends, FriendProfile? selectedFriend, bool showOnlineOnly, bool forceRefresh, Action<FriendProfile?> onRowSelected) {
        if (ImGui.BeginTable("FriendsTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable, new Vector2(tableWidth, -footerHeight))) {
            ImGui.TableSetupColumn(this.loc.Translate("Column_Status"), ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Column_Name"));
            ImGui.TableSetupColumn(this.loc.Translate("Column_Job"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Column_FC"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Column_Location"));

            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            this.HandleSortingAndFiltering(rawFriends, showOnlineOnly, forceRefresh);
            float textOffsetY = Math.Max(0, (24.0f - ImGui.GetTextLineHeight()) * 0.5f);

            foreach (var friend in this.cachedFriends) {
                ImGui.TableNextRow();

                bool isAvailable = this.gameDataService.IsFriendAvailable(friend.OnlineStateMask);
                Vector4 rowColor;

                if (friend.IsCharacterDeleted) {
                    rowColor = new Vector4(0.8f, 0.4f, 0.4f, 1.0f);
                }
                else if (friend.IsArchived) {
                    rowColor = new Vector4(0.45f, 0.45f, 0.6f, 1.0f);
                }
                else if (!friend.IsOnline) {
                    rowColor = new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
                }
                else if (!isAvailable) {
                    rowColor = new Vector4(0.75f, 0.75f, 0.75f, 1.0f);
                }
                else {
                    rowColor = new Vector4(1.0f, 1.0f, 1.0f, 1.0f);
                }

                ImGui.PushStyleColor(ImGuiCol.Text, rowColor);

                ImGui.TableNextColumn();
                float statusColWidth = ImGui.GetColumnWidth();

                var cursorStart = ImGui.GetCursorPos();
                bool isSelected = selectedFriend == friend;
                if (ImGui.Selectable($"##row_{friend.ContentId}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) {
                    onRowSelected(friend);
                }
                ImGui.SetCursorPos(cursorStart);

                if (friend.IsMissing) {
                    var iconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = 61504 };
                    var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

                    if (iconWrap != null) {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - 24.0f) * 0.5f));
                        ImGui.Image(iconWrap.Handle, new Vector2(24, 24), Vector2.Zero, Vector2.One, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(this.loc.Translate("Tooltip_MissingDeleted"));
                        }
                    }
                    else {
                        float textWidth = ImGui.CalcTextSize("X").X;
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - textWidth) * 0.5f));
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.8f, 0.2f, 0.2f, 1.0f));
                        ImGui.Text("X");
                        ImGui.PopStyleColor();
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(this.loc.Translate("Tooltip_MissingDeleted"));
                        }
                    }
                }
                else {
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
                }

                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                ImGui.Text(friend.Name);

                ImGui.TableNextColumn();
                float jobColWidth = ImGui.GetColumnWidth();

                if (friend.JobId > 0) {
                    var iconId = this.gameDataService.GetJobIconId(friend.JobId);
                    var jobAbbr = this.gameDataService.GetJobAbbreviation(friend.JobId);
                    bool iconDrawn = false;

                    if (iconId > 0) {
                        var iconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = iconId };
                        var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

                        if (iconWrap != null) {
                            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (jobColWidth - 24.0f) * 0.5f));
                            var imageTint = friend.IsOnline && !friend.IsCharacterDeleted && !friend.IsArchived ? rowColor : new Vector4(0.5f, 0.5f, 0.5f, 1.0f);
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

                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                if (!string.IsNullOrEmpty(friend.FcTag)) {
                    ImGui.Text($"<{friend.FcTag}>");
                }
                else {
                    ImGui.Text(string.Empty);
                }

                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                var locationName = this.gameDataService.GetLocationName(friend.LocationId);

                if ((string.IsNullOrEmpty(locationName) || locationName == friend.LocationId.ToString()) && friend.IsOnline) {
                    uint displayWorld = friend.CurrentWorldId > 0 ? friend.CurrentWorldId : friend.HomeWorldId;
                    locationName = this.gameDataService.GetWorldName(displayWorld);
                }

                ImGui.Text(string.IsNullOrEmpty(locationName) || locationName == "0" ? this.loc.Translate("Profile_Unknown") : locationName);

                ImGui.PopStyleColor();
            }

            ImGui.EndTable();
        }
    }

    private void HandleSortingAndFiltering(IReadOnlyList<FriendProfile> rawFriends, bool showOnlineOnly, bool forceRefresh) {
        var sortSpecs = ImGui.TableGetSortSpecs();
        bool dataUpdated = this.syncService.LastSyncTime != this.lastProcessedSyncTime;

        if (sortSpecs.SpecsDirty || forceRefresh || rawFriends.Count != this.lastFriendCount || dataUpdated) {
            int sortColumn = -1;
            bool isAscending = true;

            if (sortSpecs.SpecsCount > 0) {
                var spec = sortSpecs.Specs[0];
                sortColumn = spec.ColumnIndex;
                isAscending = spec.SortDirection == ImGuiSortDirection.Ascending;
            }

            this.cachedFriends = this.displayService.ProcessFriends(rawFriends, showOnlineOnly, sortColumn, isAscending);

            sortSpecs.SpecsDirty = false;
            this.lastFriendCount = rawFriends.Count;
            this.lastProcessedSyncTime = this.syncService.LastSyncTime;
        }
    }
}