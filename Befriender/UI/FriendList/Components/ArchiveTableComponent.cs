namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Theme.Models;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class ArchiveTableComponent {
    private ILocalizationService loc;
    private IThemeService themeService;
    private IFriendActionService actionService;
    private IFriendGroupRepository groupRepository;

    private Dictionary<string, IReadOnlyList<FriendProfile>> cachedTables = new();
    private Dictionary<string, int> lastTableFriendCounts = new();

    public ArchiveTableComponent(ILocalizationService loc, IThemeService themeService, IFriendActionService actionService, IFriendGroupRepository groupRepository) {
        this.loc = loc;
        this.themeService = themeService;
        this.actionService = actionService;
        this.groupRepository = groupRepository;
    }

    public void Draw(float tableWidth, IReadOnlyList<FriendProfile> archivedFriends, FriendProfile? selectedFriend, bool groupByGroups, Action<FriendProfile?> onRowSelected) {
        var palette = this.themeService.CurrentPalette;

        if (groupByGroups) {
            if (ImGui.BeginChild("GroupedArchiveContainer", new Vector2(tableWidth, 0))) {
                var groupsList = this.groupRepository.GetGroups();
                var groupsDict = groupsList.ToDictionary(g => g.Id, g => g.Title);
                var groupOrder = groupsList.Select(g => g.Id).ToList();

                var groupedFriends = archivedFriends
                    .GroupBy(f => f.CustomGroupId)
                    .OrderBy(g => g.Key.HasValue && groupOrder.Contains(g.Key.Value) ? groupOrder.IndexOf(g.Key.Value) : int.MaxValue);

                foreach (var group in groupedFriends) {
                    string groupName = group.Key.HasValue && groupsDict.TryGetValue(group.Key.Value, out var name) && !string.IsNullOrEmpty(name)
                        ? name
                        : this.loc.Translate("Group_Unassigned");

                    string headerText = $"{groupName} ({group.Count()})###ArchiveHeader_{group.Key}";

                    if (ImGui.CollapsingHeader(headerText, ImGuiTreeNodeFlags.DefaultOpen)) {
                        this.DrawArchiveTable($"ArchiveTable_{group.Key}", group, 0, selectedFriend, palette, onRowSelected, false);
                    }
                }
            }
            ImGui.EndChild();
        }
        else {
            this.DrawArchiveTable("ArchiveTable_All", archivedFriends, tableWidth, selectedFriend, palette, onRowSelected, true);
        }
    }

    private void DrawArchiveTable(string tableId, IEnumerable<FriendProfile> friends, float tableWidth, FriendProfile? selectedFriend, ThemePalette palette, Action<FriendProfile?> onRowSelected, bool useScroll) {
        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable;
        if (useScroll) {
            flags |= ImGuiTableFlags.ScrollY;
        }

        if (ImGui.BeginTable(tableId, 3, flags, new Vector2(tableWidth, 0))) {
            ImGui.TableSetupColumn(this.loc.Translate("Column_Name"), ImGuiTableColumnFlags.DefaultSort);
            ImGui.TableSetupColumn(this.loc.Translate("Column_AddedDate"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Column_ArchivedDate"), ImGuiTableColumnFlags.WidthFixed);

            if (useScroll) {
                ImGui.TableSetupScrollFreeze(0, 1);
            }

            ImGui.TableHeadersRow();

            var sortSpecs = ImGui.TableGetSortSpecs();
            var friendsList = friends.ToList();

            if (sortSpecs.SpecsDirty || !this.lastTableFriendCounts.TryGetValue(tableId, out int count) || count != friendsList.Count) {
                int sortColumn = 1;
                bool isAscending = false;

                if (sortSpecs.SpecsCount > 0) {
                    var spec = sortSpecs.Specs[0];
                    sortColumn = spec.ColumnIndex;
                    isAscending = spec.SortDirection == ImGuiSortDirection.Ascending;
                }

                var query = friendsList.AsEnumerable();
                query = sortColumn switch {
                    0 => isAscending ? query.OrderBy(f => f.Name) : query.OrderByDescending(f => f.Name),
                    1 => isAscending ? query.OrderBy(f => f.AddedAt).ThenBy(f => f.Name) : query.OrderByDescending(f => f.AddedAt).ThenBy(f => f.Name),
                    2 => isAscending ? query.OrderBy(f => f.ArchivedAt).ThenBy(f => f.Name) : query.OrderByDescending(f => f.ArchivedAt).ThenBy(f => f.Name),
                    _ => query
                };

                this.cachedTables[tableId] = query.ToList();
                this.lastTableFriendCounts[tableId] = friendsList.Count;
                sortSpecs.SpecsDirty = false;
            }

            if (this.cachedTables.TryGetValue(tableId, out var tableFriends)) {
                foreach (var friend in tableFriends) {
                    this.DrawArchiveRow(friend, selectedFriend, palette, onRowSelected);
                }
            }

            ImGui.EndTable();
        }
    }

    private void DrawArchiveRow(FriendProfile friend, FriendProfile? selectedFriend, ThemePalette palette, Action<FriendProfile?> onRowSelected) {
        ImGui.TableNextRow();
        ImGui.PushStyleColor(ImGuiCol.Text, palette.TextArchived);

        // --- COLUMN: NAME ---
        ImGui.TableNextColumn();
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

        float textOffsetY = Math.Max(0, (24.0f - ImGui.GetTextLineHeight()) * 0.5f);
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);

        string displayName = friend.Name;
        if (friend.IsCharacterDeleted) {
            displayName = this.loc.Translate("Profile_DeletedCharacter");
            if (friend.PreviousNames != null && friend.PreviousNames.Count > 0) {
                displayName += $" ({friend.PreviousNames[0]})";
            }
        }
        ImGui.Text(displayName);

        // --- COLUMN: ADDED DATE ---
        ImGui.TableNextColumn();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
        var addedDateStr = friend.AddedAt == DateTime.MinValue ? this.loc.Translate("Profile_Unknown") : friend.AddedAt.ToShortDateString();
        ImGui.Text(addedDateStr);

        // --- COLUMN: ARCHIVED DATE ---
        ImGui.TableNextColumn();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
        var archivedDateStr = friend.ArchivedAt == DateTime.MinValue ? this.loc.Translate("Profile_Unknown") : friend.ArchivedAt.ToShortDateString();
        ImGui.Text(archivedDateStr);

        ImGui.PopStyleColor();
    }
}