namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.Theme.Contracts;
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
    private IThemeService themeService;
    private IFriendActionService actionService;

    private IReadOnlyList<FriendProfile> cachedFriends = new List<FriendProfile>();
    private int lastFriendCount = -1;
    private DateTime lastProcessedSyncTime = DateTime.MinValue;

    public FriendListTableComponent(IFriendDisplayService displayService, IFriendSyncService syncService, IGameDataService gameDataService, ITextureProvider textureProvider, ILocalizationService loc, IThemeService themeService, IFriendActionService actionService) {
        this.displayService = displayService;
        this.syncService = syncService;
        this.gameDataService = gameDataService;
        this.textureProvider = textureProvider;
        this.loc = loc;
        this.themeService = themeService;
        this.actionService = actionService;
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
            var palette = this.themeService.CurrentPalette;

            foreach (var friend in this.cachedFriends) {
                ImGui.TableNextRow();

                bool isAvailable = this.gameDataService.IsFriendAvailable(friend.OnlineStateMask);
                Vector4 rowColor;

                if (friend.IsCharacterDeleted) {
                    rowColor = palette.TextDeleted;
                }
                else if (friend.IsArchived) {
                    rowColor = palette.TextArchived;
                }
                else if (!friend.IsOnline) {
                    rowColor = palette.TextOffline;
                }
                else if (!isAvailable) {
                    rowColor = palette.TextBusy;
                }
                else {
                    rowColor = palette.TextOnline;
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

                if (ImGui.BeginPopupContextItem($"ContextMenu_{friend.ContentId}")) {
                    var actions = this.actionService.GetAvailableActions(friend);
                    if (actions.Count == 0) {
                        ImGui.MenuItem(this.loc.Translate("Action_NoneAvailable"), false);
                    }

                    foreach (var action in actions) {
                        if (ImGui.MenuItem(this.loc.Translate(action.InternalName))) {
                            action.Execute(friend);
                        }
                    }
                    ImGui.EndPopup();
                }

                if (friend.IsMissing) {
                    var iconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = 61504 };
                    var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

                    if (iconWrap != null) {
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - 24.0f) * 0.5f));
                        ImGui.Image(iconWrap.Handle, new Vector2(24, 24), Vector2.Zero, Vector2.One, palette.IconDeletedTint);
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(this.loc.Translate("Tooltip_MissingDeleted"));
                        }
                    }
                    else {
                        float textWidth = ImGui.CalcTextSize("X").X;
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - textWidth) * 0.5f));
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                        ImGui.PushStyleColor(ImGuiCol.Text, palette.StatusFallbackDeleted);
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
                        ImGui.Image(iconWrap.Handle, new Vector2(24, 24), Vector2.Zero, Vector2.One, palette.IconDefaultTint);
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(statusInfo.Name);
                        }
                    }
                    else {
                        float textWidth = ImGui.CalcTextSize("●").X;
                        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - textWidth) * 0.5f));
                        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                        Vector4 fallbackColor = friend.IsOnline ? palette.StatusFallbackOnline : palette.StatusFallbackOffline;

                        ImGui.PushStyleColor(ImGuiCol.Text, fallbackColor);
                        ImGui.Text("●");
                        ImGui.PopStyleColor();
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(statusInfo.Name);
                        }
                    }
                }

                // --- COLUMN: NAME ---
                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);

                if (friend.IsCharacterDeleted) {
                    ImGui.PushStyleColor(ImGuiCol.Text, palette.IconDimmedTint);

                    string displayName = this.loc.Translate("Profile_DeletedCharacter");
                    if (friend.PreviousNames != null && friend.PreviousNames.Count > 0) {
                        displayName += $" ({friend.PreviousNames[0]})";
                    }

                    ImGui.Text(displayName);
                    ImGui.PopStyleColor();
                }
                else {
                    ImGui.Text(friend.Name);
                }

                // Render note icon if the friend has a note
                if (!string.IsNullOrWhiteSpace(friend.Notes)) {
                    ImGui.SameLine();

                    ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
                    ImGui.TextDisabled(((char)Dalamud.Interface.FontAwesomeIcon.StickyNote).ToString());
                    ImGui.PopFont();

                    if (ImGui.IsItemHovered()) {
                        ImGui.SetTooltip(friend.Notes);
                    }
                }

                // --- COLONNE : JOB ---
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
                            var imageTint = friend.IsOnline && !friend.IsCharacterDeleted && !friend.IsArchived ? palette.IconDefaultTint : palette.IconDimmedTint;
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

                // --- COLONNE : COMPAGNIE LIBRE ---
                ImGui.TableNextColumn();
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                if (!string.IsNullOrEmpty(friend.FcTag)) {
                    var gcIconId = this.gameDataService.GetGrandCompanyIconId(friend.GrandCompany);
                    if (gcIconId > 0) {
                        var gcIconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = gcIconId };
                        var gcIconWrap = this.textureProvider.GetFromGameIcon(gcIconLookup).GetWrapOrDefault();

                        if (gcIconWrap != null) {
                            float iconSize = ImGui.GetTextLineHeight();
                            float currentY = ImGui.GetCursorPosY(); // Sauvegarde de l'alignement centré

                            ImGui.Image(gcIconWrap.Handle, new Vector2(iconSize, iconSize));
                            ImGui.SameLine(0, 4f);

                            ImGui.SetCursorPosY(currentY); // Restauration pour le texte qui suit
                        }
                    }
                    ImGui.Text(friend.FcTag);
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