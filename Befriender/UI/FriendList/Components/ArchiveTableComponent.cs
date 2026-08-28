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

    private IReadOnlyList<FriendProfile> cachedFriends = new List<FriendProfile>();
    private int lastFriendCount = -1;

    public ArchiveTableComponent(ILocalizationService loc, IThemeService themeService, IFriendActionService actionService, IFriendGroupRepository groupRepository) {
        this.loc = loc;
        this.themeService = themeService;
        this.actionService = actionService;
        this.groupRepository = groupRepository;
    }

    public void Draw(float tableWidth, float footerHeight, IReadOnlyList<FriendProfile> archivedFriends, FriendProfile? selectedFriend, bool groupByGroups, Action<FriendProfile?> onRowSelected) {
        if (ImGui.BeginTable("ArchiveTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Sortable, new Vector2(tableWidth, -footerHeight))) {
            ImGui.TableSetupColumn(this.loc.Translate("Column_Name"), ImGuiTableColumnFlags.DefaultSort);
            ImGui.TableSetupColumn(this.loc.Translate("Column_AddedDate"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Column_ArchivedDate"), ImGuiTableColumnFlags.WidthFixed);

            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            this.HandleSorting(archivedFriends);
            var palette = this.themeService.CurrentPalette;

            if (groupByGroups) {
                var groupsDict = this.groupRepository.GetGroups().ToDictionary(g => g.Id, g => g.Title);

                var groupedFriends = this.cachedFriends
                    .GroupBy(f => f.CustomGroupId)
                    .OrderBy(g => g.Key.HasValue ? (groupsDict.TryGetValue(g.Key.Value, out var title) ? title : string.Empty) : "ZZZZZ_UNASSIGNED");

                foreach (var group in groupedFriends) {
                    ImGui.TableNextRow();
                    ImGui.TableNextColumn();

                    string groupName = group.Key.HasValue && groupsDict.TryGetValue(group.Key.Value, out var name) && !string.IsNullOrEmpty(name)
                        ? name
                        : this.loc.Translate("Group_Unassigned");

                    string headerText = $"{groupName} ({group.Count()})";

                    ImGui.PushStyleColor(ImGuiCol.Text, palette.Text);
                    bool isNodeOpen = ImGui.TreeNodeEx(headerText, ImGuiTreeNodeFlags.SpanFullWidth | ImGuiTreeNodeFlags.DefaultOpen);
                    ImGui.PopStyleColor();

                    if (isNodeOpen) {
                        foreach (var friend in group) {
                            this.DrawArchiveRow(friend, selectedFriend, palette, onRowSelected);
                        }

                        ImGui.TreePop();
                    }
                }
            }
            else {
                foreach (var friend in this.cachedFriends) {
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

    private void HandleSorting(IReadOnlyList<FriendProfile> archivedFriends) {
        var sortSpecs = ImGui.TableGetSortSpecs();

        if (sortSpecs.SpecsDirty || archivedFriends.Count != this.lastFriendCount) {
            int sortColumn = 1; // Default sort by Added Date
            bool isAscending = false;

            if (sortSpecs.SpecsCount > 0) {
                var spec = sortSpecs.Specs[0];
                sortColumn = spec.ColumnIndex;
                isAscending = spec.SortDirection == ImGuiSortDirection.Ascending;
            }

            var query = archivedFriends.AsEnumerable();
            query = sortColumn switch {
                0 => isAscending ? query.OrderBy(f => f.Name) : query.OrderByDescending(f => f.Name),
                1 => isAscending ? query.OrderBy(f => f.AddedAt).ThenBy(f => f.Name) : query.OrderByDescending(f => f.AddedAt).ThenBy(f => f.Name),
                2 => isAscending ? query.OrderBy(f => f.ArchivedAt).ThenBy(f => f.Name) : query.OrderByDescending(f => f.ArchivedAt).ThenBy(f => f.Name),
                _ => query
            };

            this.cachedFriends = query.ToList();
            sortSpecs.SpecsDirty = false;
            this.lastFriendCount = archivedFriends.Count;
        }
    }
}