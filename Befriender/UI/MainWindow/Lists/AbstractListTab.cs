namespace Befriender.UI.MainWindow.Lists;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Contracts;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.MainWindow.Contracts;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Theme.Models;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public abstract class AbstractListTab : ITab, IDisposable {
    protected ICharacterRegistry registry;
    protected ILocalizationService loc;
    protected IGameDataService gameDataService;
    protected IThemeService themeService;
    protected ITextureProvider textureProvider;
    protected IProximityService proximityService;
    protected ICharacterActionService actionService;
    protected ICharacterGroupRepository groupRepository;
    protected ICharacterTagRepository tagRepository;
    protected IConfigurationService configurationService;
    protected ListToolbarComponent toolbarComponent;
    protected CharacterProfilePanelComponent profilePanelComponent;

    protected string searchQuery = string.Empty;
    protected bool showOnlineOnly = false;
    protected bool showNearbyOnly = false;
    protected bool groupByGroups = false;
    protected bool isFiltersExpanded = false;

    protected Character? selectedCharacter = null;
    protected const float PanelWidth = 300f;

    protected List<Character> cachedCharacterList = new();
    protected bool requiresListRebuild = true;

    public abstract string InternalName { get; }
    public abstract string Name { get; }
    public bool IsProfilePanelOpen => this.selectedCharacter != null;

    protected abstract string EmptyListMessageKey { get; }
    protected abstract IEnumerable<Character> GetBaseCharacterList();

    protected virtual bool ShowOnlineFilter => true;

    protected AbstractListTab(
        ICharacterRegistry registry,
        ILocalizationService loc,
        IGameDataService gameDataService,
        IThemeService themeService,
        ITextureProvider textureProvider,
        IProximityService proximityService,
        ICharacterActionService actionService,
        ICharacterGroupRepository groupRepository,
        ICharacterTagRepository tagRepository,
        ListToolbarComponent toolbarComponent,
        CharacterProfilePanelComponent profilePanelComponent,
        IConfigurationService configurationService) {

        this.registry = registry;
        this.loc = loc;
        this.gameDataService = gameDataService;
        this.themeService = themeService;
        this.textureProvider = textureProvider;
        this.proximityService = proximityService;
        this.actionService = actionService;
        this.groupRepository = groupRepository;
        this.tagRepository = tagRepository;
        this.toolbarComponent = toolbarComponent;
        this.profilePanelComponent = profilePanelComponent;
        this.configurationService = configurationService;

        var config = this.configurationService.GetConfig();
        if (config.TabStates.TryGetValue(this.InternalName, out var state)) {
            this.showOnlineOnly = state.ShowOnlineOnly;
            this.showNearbyOnly = state.ShowNearbyOnly;
            this.groupByGroups = state.GroupByGroups;
            this.isFiltersExpanded = state.IsFiltersExpanded;
        }

        this.registry.RegistryUpdated += this.OnRegistryUpdated;
    }

    private void OnRegistryUpdated() {
        this.requiresListRebuild = true;
    }

    protected virtual IEnumerable<Character> SortCharacterList(IEnumerable<Character> characters) {
        return characters.OrderByDescending(c => c.IsOnline).ThenBy(c => c.Name);
    }

    private void RebuildCache() {
        var baseList = this.GetBaseCharacterList();
        if (baseList == null) {
            this.cachedCharacterList = new List<Character>();
            return;
        }

        if (this.showOnlineOnly) {
            baseList = baseList.Where(m => m.IsOnline);
        }

        if (this.showNearbyOnly) {
            baseList = baseList.Where(m => this.proximityService.IsFriendNearby(m.ContentId));
        }

        if (!string.IsNullOrWhiteSpace(this.searchQuery)) {
            var allTags = this.tagRepository.GetTags();
            var query = this.searchQuery.ToLowerInvariant();

            baseList = baseList.Where(m => {
                if (m.Name.Contains(query, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }
                if (!string.IsNullOrEmpty(m.Notes) && m.Notes.Contains(query, StringComparison.OrdinalIgnoreCase)) {
                    return true;
                }

                var characterTags = allTags.Where(t => m.Tags.Contains(t.Id));
                if (characterTags.Any(t => t.Name.Contains(query, StringComparison.OrdinalIgnoreCase))) {
                    return true;
                }

                return false;
            });
        }

        this.cachedCharacterList = this.SortCharacterList(baseList).ToList();
    }

    public void Draw() {
        var palette = this.themeService.CurrentPalette;

        bool oldOnline = this.showOnlineOnly;
        bool oldNearby = this.showNearbyOnly;
        bool oldGroup = this.groupByGroups;
        bool oldExpanded = this.isFiltersExpanded;

        bool listNeedsRefresh = this.toolbarComponent.Draw(ref this.showOnlineOnly, ref this.showNearbyOnly, ref this.groupByGroups, ref this.searchQuery, ref this.isFiltersExpanded, this.ShowOnlineFilter);

        if (oldOnline != this.showOnlineOnly || oldNearby != this.showNearbyOnly || oldGroup != this.groupByGroups || oldExpanded != this.isFiltersExpanded) {
            var config = this.configurationService.GetConfig();
            if (!config.TabStates.TryGetValue(this.InternalName, out var state)) {
                state = new TabState();
                config.TabStates[this.InternalName] = state;
            }

            state.ShowOnlineOnly = this.showOnlineOnly;
            state.ShowNearbyOnly = this.showNearbyOnly;
            state.GroupByGroups = this.groupByGroups;
            state.IsFiltersExpanded = this.isFiltersExpanded;
            this.configurationService.Save();
        }

        if (listNeedsRefresh || this.requiresListRebuild) {
            this.RebuildCache();
            this.requiresListRebuild = false;
        }

        ImGui.Separator();
        ImGui.Spacing();

        if (this.cachedCharacterList.Count == 0) {
            ImGui.TextDisabled(this.loc.Translate(this.EmptyListMessageKey));
            return;
        }

        float footerHeight = ImGui.GetFrameHeight() + (ImGui.GetStyle().ItemSpacing.Y * 3);
        float tableWidth = this.selectedCharacter != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        if (ImGui.BeginChild($"{this.InternalName}_Container", new Vector2(tableWidth, -footerHeight))) {
            float textOffsetY = Math.Max(0, (24.0f - ImGui.GetTextLineHeight()) * 0.5f);

            if (this.groupByGroups) {
                var allGroups = this.groupRepository.GetGroups();

                foreach (var group in allGroups) {
                    var groupChars = this.cachedCharacterList.Where(c => c.CustomGroupId == group.Id).ToList();
                    if (groupChars.Count == 0) {
                        continue;
                    }

                    if (ImGui.CollapsingHeader($"{group.Title} ({groupChars.Count})###Group_{group.Id}", ImGuiTreeNodeFlags.DefaultOpen)) {
                        this.DrawCharacterTable($"{this.InternalName}_Table_{group.Id}", groupChars, palette, textOffsetY, false);
                    }
                }

                var unassignedChars = this.cachedCharacterList.Where(c => c.CustomGroupId == null).ToList();
                if (unassignedChars.Count > 0) {
                    if (ImGui.CollapsingHeader($"{this.loc.Translate("Group_Unassigned")} ({unassignedChars.Count})###Group_Unassigned", ImGuiTreeNodeFlags.DefaultOpen)) {
                        this.DrawCharacterTable($"{this.InternalName}_Table_Unassigned", unassignedChars, palette, textOffsetY, false);
                    }
                }
            }
            else {
                this.DrawCharacterTable($"{this.InternalName}_Table", this.cachedCharacterList, palette, textOffsetY, true);
            }
        }
        ImGui.EndChild();

        if (this.selectedCharacter != null) {
            ImGui.SameLine();
            this.profilePanelComponent.Draw(PanelWidth, -footerHeight, this.selectedCharacter, () => this.selectedCharacter = null);
        }
    }

    private void DrawCharacterTable(string tableId, IEnumerable<Character> characters, ThemePalette palette, float textOffsetY, bool useScrollY) {
        // NOUVEAU : Ajout de ImGuiTableFlags.Sortable
        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable;

        if (useScrollY) {
            flags |= ImGuiTableFlags.ScrollY;
        }

        if (ImGui.BeginTable(tableId, 4, flags)) {
            // NOUVEAU : Attribution d'un ID d'utilisateur pour chaque colonne via le 4ème paramètre pour le moteur de tri
            ImGui.TableSetupColumn(this.loc.Translate("Column_Status"), ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.DefaultSort, 0f, 0u);
            ImGui.TableSetupColumn(this.loc.Translate("Column_Name"), ImGuiTableColumnFlags.None, 0f, 1u);
            ImGui.TableSetupColumn(this.loc.Translate("Column_Job"), ImGuiTableColumnFlags.WidthFixed, 0f, 2u);
            ImGui.TableSetupColumn(this.loc.Translate("Column_Location"), ImGuiTableColumnFlags.None, 0f, 3u);

            if (useScrollY) {
                ImGui.TableSetupScrollFreeze(0, 1);
            }

            ImGui.TableHeadersRow();

            // NOUVEAU : Lecture des spécifications de tri fournies par l'utilisateur (via un clic sur l'en-tête)
            var sortSpecs = ImGui.TableGetSortSpecs();
            IEnumerable<Character> sortedCharacters = characters;

            if (sortSpecs.SpecsCount > 0) {
                var spec = sortSpecs.Specs;
                bool isAscending = spec.SortDirection == ImGuiSortDirection.Ascending;

                sortedCharacters = spec.ColumnUserID switch {
                    0u => isAscending ? characters.OrderBy(c => c.IsOnline).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.IsOnline).ThenBy(c => c.Name),
                    1u => isAscending ? characters.OrderBy(c => c.Name) : characters.OrderByDescending(c => c.Name),
                    2u => isAscending ? characters.OrderBy(c => c.JobId).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.JobId).ThenBy(c => c.Name),
                    3u => isAscending ? characters.OrderBy(c => c.LocationId).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.LocationId).ThenBy(c => c.Name),
                    _ => characters
                };
            }

            foreach (var character in sortedCharacters) {
                this.DrawCharacterRow(character, palette, textOffsetY);
            }

            ImGui.EndTable();
        }
    }

    private void DrawCharacterRow(Character character, ThemePalette palette, float textOffsetY) {
        ImGui.TableNextRow();

        bool isAvailable = this.gameDataService.IsFriendAvailable(character.OnlineStateMask);
        bool isDeleted = string.IsNullOrEmpty(character.Name);
        Vector4 rowColor;

        if (isDeleted || !character.IsActivelyTracked) {
            rowColor = palette.TextArchived;
        }
        else if (!character.IsOnline) {
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
        bool isSelected = this.selectedCharacter == character;

        if (ImGui.Selectable($"##row_{character.ContentId}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) {
            this.selectedCharacter = isSelected ? null : character;
        }
        ImGui.SetCursorPos(cursorStart);

        if (ImGui.BeginPopupContextItem($"ContextMenu_{character.ContentId}")) {
            var actions = this.actionService.GetAvailableActions(character);
            if (actions.Count == 0) {
                ImGui.MenuItem(this.loc.Translate("Action_NoneAvailable"), false);
            }
            foreach (var action in actions) {
                if (ImGui.MenuItem(this.loc.Translate(action.InternalName))) {
                    action.Execute(character);
                }
            }
            ImGui.EndPopup();
        }

        if (isDeleted || !character.IsActivelyTracked) {
            ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
            var iconStr = isDeleted ? ((char)FontAwesomeIcon.Ghost).ToString() : ((char)FontAwesomeIcon.Archive).ToString();
            float textWidth = ImGui.CalcTextSize(iconStr).X;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - textWidth) * 0.5f));
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
            ImGui.Text(iconStr);
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip(this.loc.Translate(isDeleted ? "Profile_StatusDeleted" : "Profile_StatusArchived"));
            }
        }
        else {
            ulong effectiveMask = character.IsOnline ? character.OnlineStateMask : 0;
            var statusInfo = this.gameDataService.GetOnlineStatusInfo(effectiveMask, character.CurrentWorldId, character.HomeWorldId, character.LocationId);
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
                ImGui.PushStyleColor(ImGuiCol.Text, character.IsOnline ? palette.StatusFallbackOnline : palette.StatusFallbackOffline);
                ImGui.Text("●");
                ImGui.PopStyleColor();
                if (ImGui.IsItemHovered()) {
                    ImGui.SetTooltip(statusInfo.Name);
                }
            }
        }

        ImGui.TableNextColumn();
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);

        if (this.proximityService.IsFriendNearby(character.ContentId)) {
            ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
            ImGui.TextColored(palette.StatusFallbackOnline, ((char)FontAwesomeIcon.StreetView).ToString());
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip(this.loc.Translate("Tooltip_Nearby"));
            }
            ImGui.SameLine();
        }

        ImGui.Text(isDeleted ? this.loc.Translate("Profile_DeletedCharacter") : character.Name);

        if (character.IsTrackedForNotifications) {
            ImGui.SameLine();
            ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
            ImGui.TextColored(palette.IconDefaultTint, ((char)FontAwesomeIcon.Bell).ToString());
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip(this.loc.Translate("Tooltip_Tracked"));
            }
        }

        if (!string.IsNullOrWhiteSpace(character.Notes)) {
            ImGui.SameLine();
            ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
            ImGui.TextDisabled(((char)FontAwesomeIcon.StickyNote).ToString());
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) {
                ImGui.SetTooltip(character.Notes);
            }
        }

        ImGui.TableNextColumn();
        float jobColWidth = ImGui.GetColumnWidth();

        if (character.JobId > 0) {
            var jobIconId = this.gameDataService.GetJobIconId(character.JobId);
            var jobAbbr = this.gameDataService.GetJobAbbreviation(character.JobId);
            bool iconDrawn = false;

            if (jobIconId > 0) {
                var jIconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = jobIconId };
                var jIconWrap = this.textureProvider.GetFromGameIcon(jIconLookup).GetWrapOrDefault();

                if (jIconWrap != null) {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (jobColWidth - 24.0f) * 0.5f));
                    ImGui.Image(jIconWrap.Handle, new Vector2(24, 24), Vector2.Zero, Vector2.One, character.IsOnline ? palette.IconDefaultTint : palette.IconDimmedTint);
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

        string locationName = this.gameDataService.GetDisplayLocation(character.LocationId, character.CurrentWorldId, character.HomeWorldId, character.OnlineStateMask);
        ImGui.Text(string.IsNullOrEmpty(locationName) || locationName == "0" ? this.loc.Translate("Profile_Unknown") : locationName);

        ImGui.PopStyleColor();
    }

    public virtual void Dispose() {
        this.registry.RegistryUpdated -= this.OnRegistryUpdated;
    }
}