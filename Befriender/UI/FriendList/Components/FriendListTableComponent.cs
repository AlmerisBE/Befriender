namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Theme.Models;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class FriendListTableComponent {
    private IFriendDisplayService displayService;
    private IFriendSyncService syncService;
    private IGameDataService gameDataService;
    private ITextureProvider textureProvider;
    private ILocalizationService loc;
    private IThemeService themeService;
    private IFriendActionService actionService;
    private IFriendGroupRepository groupRepository;

    private Dictionary<string, IReadOnlyList<FriendProfile>> cachedTables = new();
    private Dictionary<string, int> lastTableFriendCounts = new();
    private DateTime lastProcessedSyncTime = DateTime.MinValue;

    public FriendListTableComponent(IFriendDisplayService displayService, IFriendSyncService syncService, IGameDataService gameDataService, ITextureProvider textureProvider, ILocalizationService loc, IThemeService themeService, IFriendActionService actionService, IFriendGroupRepository groupRepository) {
        this.displayService = displayService;
        this.syncService = syncService;
        this.gameDataService = gameDataService;
        this.textureProvider = textureProvider;
        this.loc = loc;
        this.themeService = themeService;
        this.actionService = actionService;
        this.groupRepository = groupRepository;
    }

    public void Draw(float tableWidth, IReadOnlyList<FriendProfile> rawFriends, FriendProfile? selectedFriend, bool showOnlineOnly, bool groupByGroups, bool forceRefresh, Action<FriendProfile?> onRowSelected) {
        float textOffsetY = Math.Max(0, (24.0f - ImGui.GetTextLineHeight()) * 0.5f);
        var palette = this.themeService.CurrentPalette;
        bool dataUpdated = this.syncService.LastSyncTime != this.lastProcessedSyncTime;
        bool needsRefresh = forceRefresh || dataUpdated;

        var displayFriends = showOnlineOnly ? rawFriends.Where(f => f.IsOnline).ToList() : rawFriends.ToList();

        if (groupByGroups) {
            if (ImGui.BeginChild("GroupedListContainer", new Vector2(tableWidth, 0))) {
                var groupsDict = this.groupRepository.GetGroups().ToDictionary(g => g.Id, g => g.Title);

                var groupedFriends = displayFriends
                    .GroupBy(f => f.CustomGroupId)
                    .OrderBy(g => g.Key.HasValue ? (groupsDict.TryGetValue(g.Key.Value, out var title) ? title : string.Empty) : "ZZZZZ_UNASSIGNED");

                foreach (var group in groupedFriends) {
                    string groupName = group.Key.HasValue && groupsDict.TryGetValue(group.Key.Value, out var name) && !string.IsNullOrEmpty(name)
                        ? name
                        : this.loc.Translate("Group_Unassigned");

                    string headerText = $"{groupName} ({group.Count()})###GroupHeader_{group.Key}";

                    if (ImGui.CollapsingHeader(headerText, ImGuiTreeNodeFlags.DefaultOpen)) {
                        this.DrawFriendTable($"FriendsTable_{group.Key}", group, 0, selectedFriend, palette, textOffsetY, needsRefresh, onRowSelected, false);
                    }
                }
            }
            ImGui.EndChild();
        }
        else {
            this.DrawFriendTable("FriendsTable_All", displayFriends, tableWidth, selectedFriend, palette, textOffsetY, needsRefresh, onRowSelected, true);
        }

        this.lastProcessedSyncTime = this.syncService.LastSyncTime;
    }

    private void DrawFriendTable(string tableId, IEnumerable<FriendProfile> friends, float tableWidth, FriendProfile? selectedFriend, ThemePalette palette, float textOffsetY, bool needsRefresh, Action<FriendProfile?> onRowSelected, bool useScroll) {
        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable;
        if (useScroll) {
            flags |= ImGuiTableFlags.ScrollY;
        }

        if (ImGui.BeginTable(tableId, 5, flags, new Vector2(tableWidth, 0))) {
            ImGui.TableSetupColumn(this.loc.Translate("Column_Status"), ImGuiTableColumnFlags.DefaultSort | ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Column_Name"));
            ImGui.TableSetupColumn(this.loc.Translate("Column_Job"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Column_FC"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Column_Location"));

            if (useScroll) {
                ImGui.TableSetupScrollFreeze(0, 1);
            }

            ImGui.TableHeadersRow();

            var sortSpecs = ImGui.TableGetSortSpecs();
            var friendsList = friends.ToList();

            if (sortSpecs.SpecsDirty || needsRefresh || !this.lastTableFriendCounts.TryGetValue(tableId, out int count) || count != friendsList.Count) {
                int sortColumn = -1;
                bool isAscending = true;

                if (sortSpecs.SpecsCount > 0) {
                    var spec = sortSpecs.Specs[0];
                    sortColumn = spec.ColumnIndex;
                    isAscending = spec.SortDirection == ImGuiSortDirection.Ascending;
                }

                this.cachedTables[tableId] = this.displayService.ProcessFriends(friendsList, false, sortColumn, isAscending);
                this.lastTableFriendCounts[tableId] = friendsList.Count;
                sortSpecs.SpecsDirty = false;
            }

            if (this.cachedTables.TryGetValue(tableId, out var tableFriends)) {
                foreach (var friend in tableFriends) {
                    this.DrawFriendRow(friend, selectedFriend, palette, textOffsetY, onRowSelected);
                }
            }

            ImGui.EndTable();
        }
    }

    private void DrawFriendRow(FriendProfile friend, FriendProfile? selectedFriend, ThemePalette palette, float textOffsetY, Action<FriendProfile?> onRowSelected) {
        ImGui.TableNextRow();

        bool isAvailable = this.gameDataService.IsFriendAvailable(friend.OnlineStateMask);
        Vector4 rowColor;

        if (friend.IsMarkedForRemoval) {
            rowColor = palette.TextMarkedForRemoval;
        }
        else if (friend.IsCharacterDeleted) {
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
            var statusInfo = this.gameDataService.GetOnlineStatusInfo(effectiveMask, friend.CurrentWorldId, friend.HomeWorldId, friend.LocationId);
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

        if (friend.IsTrackedForNotifications) {
            ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
            ImGui.Text(((char)Dalamud.Interface.FontAwesomeIcon.Bell).ToString());
            ImGui.PopFont();

            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip(this.loc.Translate("Tooltip_Tracked"));
            }

            ImGui.SameLine();
        }

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
                    float currentY = ImGui.GetCursorPosY();

                    ImGui.Image(gcIconWrap.Handle, new Vector2(iconSize, iconSize));
                    ImGui.SameLine(0, 4f);
                    ImGui.SetCursorPosY(currentY);
                }
            }
            ImGui.Text(friend.FcTag);
        }
        else {
            ImGui.Text(string.Empty);
        }

        // --- COLONNE : LIEU ---
        ImGui.TableNextColumn();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);

        string locationName = string.Empty;
        if (friend.IsOnline) {
            locationName = this.gameDataService.GetDisplayLocation(friend.LocationId, friend.CurrentWorldId, friend.HomeWorldId, friend.OnlineStateMask);
        }

        ImGui.Text(string.IsNullOrEmpty(locationName) || locationName == "0" ? this.loc.Translate("Profile_Unknown") : locationName);

        ImGui.PopStyleColor();
    }
}