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
    protected IGameGui gameGui;
    protected ICharacterSearchService searchService;

    protected string searchQuery = string.Empty;
    protected int statusFilter = 0;
    protected bool showNearbyOnly = false;
    protected bool groupByGroups = false;
    protected bool isFiltersExpanded = false;

    protected Character? selectedCharacter = null;
    protected const float PanelWidth = 300f;

    protected List<Character> cachedCharacterList = new();
    protected bool requiresListRebuild = true;

    public abstract string InternalName { get; }
    public abstract string Name { get; }
    public abstract int Order { get; }
    public bool IsProfilePanelOpen => this.selectedCharacter != null;

    protected abstract string EmptyListMessageKey { get; }
    protected abstract IEnumerable<Character> GetBaseCharacterList();
    protected virtual bool ShowStatusFilter => true;

    protected List<ColumnType> activeColumns = new() { ColumnType.Status, ColumnType.Name, ColumnType.Job, ColumnType.Location };

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
        IConfigurationService configurationService,
        IGameGui gameGui,
        ICharacterSearchService searchService) {

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
        this.gameGui = gameGui;
        this.searchService = searchService;

        var config = this.configurationService.GetConfig();
        if (config.TabStates.TryGetValue(this.InternalName, out var state)) {
            this.statusFilter = state.StatusFilter;
            this.showNearbyOnly = state.ShowNearbyOnly;
            this.groupByGroups = state.GroupByGroups;
            this.isFiltersExpanded = state.IsFiltersExpanded;

            if (state.ActiveColumns != null && state.ActiveColumns.Count > 0) {
                this.activeColumns = state.ActiveColumns.Distinct().ToList();
            }
            else {
                this.activeColumns = new List<ColumnType> { ColumnType.Status, ColumnType.Name, ColumnType.Job, ColumnType.Location };
            }
        }

        this.registry.RegistryUpdated += this.OnRegistryUpdated;
        this.configurationService.ConfigSaved += this.OnConfigSaved;
    }

    private void OnRegistryUpdated() {
        this.requiresListRebuild = true;
        if (this.selectedCharacter != null && this.registry.GetCharacterById(this.selectedCharacter.Id) == null) {
            this.selectedCharacter = null;
        }
    }

    private void OnConfigSaved() {
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

        if (this.statusFilter == 1) baseList = baseList.Where(m => m.IsOnline);
        else if (this.statusFilter == 2) baseList = baseList.Where(m => !m.IsOnline);
        else if (this.statusFilter == 3) baseList = baseList.Where(m => !m.IsActivelyTracked);

        if (this.showNearbyOnly) baseList = baseList.Where(m => this.proximityService.IsFriendNearby(m.ContentId));

        if (!string.IsNullOrWhiteSpace(this.searchQuery)) {
            baseList = this.searchService.FilterCharacters(baseList, this.searchQuery);
        }

        this.cachedCharacterList = this.SortCharacterList(baseList).ToList();
    }

    public void Draw() {
        var palette = this.themeService.CurrentPalette;
        int oldStatus = this.statusFilter;
        bool oldNearby = this.showNearbyOnly;
        bool oldGroup = this.groupByGroups;
        bool oldExpanded = this.isFiltersExpanded;
        var oldColumns = this.activeColumns.ToList();

        bool listNeedsRefresh = this.toolbarComponent.Draw(ref this.statusFilter, ref this.showNearbyOnly, ref this.groupByGroups, ref this.searchQuery, ref this.isFiltersExpanded, this.ShowStatusFilter, this.activeColumns);

        bool columnsChanged = !oldColumns.SequenceEqual(this.activeColumns);

        if (oldStatus != this.statusFilter || oldNearby != this.showNearbyOnly || oldGroup != this.groupByGroups || oldExpanded != this.isFiltersExpanded || columnsChanged) {
            var config = this.configurationService.GetConfig();
            if (!config.TabStates.TryGetValue(this.InternalName, out var state)) {
                state = new TabState();
                config.TabStates[this.InternalName] = state;
            }

            state.StatusFilter = this.statusFilter;
            state.ShowNearbyOnly = this.showNearbyOnly;
            state.GroupByGroups = this.groupByGroups;
            state.IsFiltersExpanded = this.isFiltersExpanded;
            state.ActiveColumns = this.activeColumns.ToList();

            this.configurationService.Save();
        }

        if (listNeedsRefresh || this.requiresListRebuild) {
            this.RebuildCache();
            this.requiresListRebuild = false;
        }

        ImGui.Separator();
        ImGui.Spacing();

        float footerHeight = ImGui.GetFrameHeight() + (ImGui.GetStyle().ItemSpacing.Y * 3);
        float tableWidth = this.selectedCharacter != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        if (ImGui.BeginChild($"{this.InternalName}_Container", new Vector2(tableWidth, -footerHeight))) {
            if (this.cachedCharacterList.Count == 0) {
                ImGui.TextDisabled(this.loc.Translate(this.EmptyListMessageKey));
            }
            else {
                float textOffsetY = Math.Max(0, (24.0f - ImGui.GetTextLineHeight()) * 0.5f);

                if (this.groupByGroups) {
                    var allGroups = this.groupRepository.GetGroups();

                    foreach (var group in allGroups) {
                        var groupChars = this.cachedCharacterList.Where(c => c.CustomGroupId == group.Id).ToList();
                        if (groupChars.Count == 0) continue;

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
        }
        ImGui.EndChild();

        if (this.selectedCharacter != null) {
            ImGui.SameLine();
            this.profilePanelComponent.Draw(PanelWidth, -footerHeight, this.selectedCharacter, () => this.selectedCharacter = null);
        }
    }

    private void DrawCharacterTable(string tableId, IEnumerable<Character> characters, ThemePalette palette, float textOffsetY, bool useScrollY) {
        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Sortable;
        if (useScrollY) flags |= ImGuiTableFlags.ScrollY;

        if (ImGui.BeginTable(tableId, this.activeColumns.Count, flags)) {
            foreach (var col in this.activeColumns) {
                var colFlags = ImGuiTableColumnFlags.None;
                float width = 0f;

                if (col == ColumnType.Status || col == ColumnType.Job || col == ColumnType.Level || col == ColumnType.GrandCompany) {
                    colFlags |= ImGuiTableColumnFlags.WidthFixed;
                }
                if (col == ColumnType.Status) {
                    colFlags |= ImGuiTableColumnFlags.DefaultSort;
                }

                ImGui.TableSetupColumn(this.loc.Translate($"Column_{col}"), colFlags, width, (uint)col);
            }

            if (useScrollY) ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            var sortSpecs = ImGui.TableGetSortSpecs();
            IEnumerable<Character> sortedCharacters = characters;

            if (sortSpecs.SpecsCount > 0) {
                var spec = sortSpecs.Specs;
                bool isAscending = spec.SortDirection == ImGuiSortDirection.Ascending;
                ColumnType sortCol = (ColumnType)spec.ColumnUserID;

                sortedCharacters = sortCol switch {
                    ColumnType.Status => isAscending ? characters.OrderBy(c => c.IsOnline).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.IsOnline).ThenBy(c => c.Name),
                    ColumnType.Name => isAscending ? characters.OrderBy(c => c.Name) : characters.OrderByDescending(c => c.Name),
                    ColumnType.Job => isAscending ? characters.OrderBy(c => c.JobId).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.JobId).ThenBy(c => c.Name),
                    ColumnType.Location => isAscending ? characters.OrderBy(c => c.LocationId).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.LocationId).ThenBy(c => c.Name),
                    ColumnType.LastSeen => isAscending ? characters.OrderBy(c => c.LastSeenAt).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.LastSeenAt).ThenBy(c => c.Name),
                    ColumnType.Level => isAscending ? characters.OrderBy(c => c.Level).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.Level).ThenBy(c => c.Name),
                    ColumnType.FreeCompany => isAscending ? characters.OrderBy(c => c.FcTag).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.FcTag).ThenBy(c => c.Name),
                    ColumnType.ClientLanguages => isAscending ? characters.OrderBy(c => c.ClientLanguages).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.ClientLanguages).ThenBy(c => c.Name),
                    ColumnType.GrandCompany => isAscending ? characters.OrderBy(c => c.GrandCompany).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.GrandCompany).ThenBy(c => c.Name),
                    ColumnType.DateAdded => isAscending ? characters.OrderBy(c => c.AddedAt).ThenBy(c => c.Name) : characters.OrderByDescending(c => c.AddedAt).ThenBy(c => c.Name),
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
        bool isNearby = this.proximityService.IsFriendNearby(character.ContentId);

        Vector4 rowColor = palette.TextOnline;
        if (isDeleted || (!character.IsActivelyTracked && !isNearby)) rowColor = palette.TextArchived;
        else if (!character.IsOnline) rowColor = palette.TextOffline;
        else if (!isAvailable) rowColor = palette.TextBusy;

        ImGui.PushStyleColor(ImGuiCol.Text, rowColor);

        foreach (var col in this.activeColumns) {
            ImGui.TableNextColumn();

            switch (col) {
                case ColumnType.Status:
                    this.DrawStatusCell(character, palette, textOffsetY, isDeleted, isNearby);
                    break;
                case ColumnType.Name:
                    this.DrawNameCell(character, palette, textOffsetY, isDeleted, isNearby);
                    break;
                case ColumnType.Job:
                    this.DrawJobCell(character, palette, textOffsetY);
                    break;
                case ColumnType.Location:
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                    string locationName = this.gameDataService.GetDisplayLocation(character.LocationId, character.CurrentWorldId, character.HomeWorldId, character.OnlineStateMask);
                    ImGui.Text(string.IsNullOrEmpty(locationName) || locationName == "0" ? this.loc.Translate("Profile_Unknown") : locationName);
                    break;
                case ColumnType.LastSeen:
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                    if (character.IsOnline) ImGui.Text(this.loc.Translate("Profile_Online"));
                    else if (character.LastSeenAt == DateTime.MinValue) ImGui.Text(this.loc.Translate("Profile_Unknown"));
                    else ImGui.Text(character.LastSeenAt.ToString("g"));
                    break;
                case ColumnType.Level:
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                    ImGui.Text(character.Level > 0 ? character.Level.ToString() : string.Empty);
                    break;
                case ColumnType.FreeCompany:
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                    ImGui.Text(character.FcTag);
                    break;
                case ColumnType.ClientLanguages:
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                    ImGui.Text(this.gameDataService.GetClientLanguageString(character.ClientLanguages));
                    break;
                case ColumnType.GrandCompany:
                    this.DrawGrandCompanyCell(character, palette, textOffsetY);
                    break;
                case ColumnType.DateAdded:
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                    ImGui.Text(character.AddedAt == DateTime.MinValue ? this.loc.Translate("Profile_Unknown") : character.AddedAt.ToShortDateString());
                    break;
            }
        }

        ImGui.PopStyleColor();
    }

    private void DrawStatusCell(Character character, ThemePalette palette, float textOffsetY, bool isDeleted, bool isNearby) {
        float statusColWidth = ImGui.GetColumnWidth();
        var cursorStart = ImGui.GetCursorPos();
        bool isSelected = this.selectedCharacter == character;

        if (ImGui.Selectable($"##row_{character.ContentId}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) {
            this.selectedCharacter = isSelected ? null : character;
        }
        ImGui.SetCursorPos(cursorStart);

        this.DrawContextMenu(character);

        if (isDeleted || (!character.IsActivelyTracked && !isNearby)) {
            ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
            var iconStr = isDeleted ? ((char)FontAwesomeIcon.Ghost).ToString() : ((char)FontAwesomeIcon.QuestionCircle).ToString();
            float textWidth = ImGui.CalcTextSize(iconStr).X;
            ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - textWidth) * 0.5f));
            ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
            ImGui.Text(iconStr);
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.loc.Translate(isDeleted ? "Profile_StatusDeleted" : "Profile_StatusUnsynchronized"));
        }
        else {
            var statusInfo = this.gameDataService.GetOnlineStatusInfo(character.IsOnline, character.OnlineStateMask, character.OnlineStatusId, character.CurrentWorldId, character.HomeWorldId, character.LocationId);
            var iconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = statusInfo.IconId };
            var iconWrap = this.textureProvider.GetFromGameIcon(iconLookup).GetWrapOrDefault();

            if (iconWrap != null) {
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - 24.0f) * 0.5f));
                ImGui.Image(iconWrap.Handle, new Vector2(24, 24), Vector2.Zero, Vector2.One, palette.IconDefaultTint);
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(statusInfo.Name);
            }
            else {
                float textWidth = ImGui.CalcTextSize("●").X;
                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (statusColWidth - textWidth) * 0.5f));
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                ImGui.PushStyleColor(ImGuiCol.Text, character.IsOnline ? palette.StatusFallbackOnline : palette.StatusFallbackOffline);
                ImGui.Text("●");
                ImGui.PopStyleColor();
                if (ImGui.IsItemHovered()) ImGui.SetTooltip(statusInfo.Name);
            }
        }
    }

    private void DrawContextMenu(Character character) {
        if (ImGui.BeginPopupContextItem($"ContextMenu_{character.ContentId}")) {
            var actions = this.actionService.GetAvailableActions(character);
            if (actions.Count == 0) ImGui.MenuItem(this.loc.Translate("Action_NoneAvailable"), false);

            foreach (var action in actions) {
                if (ImGui.MenuItem(this.loc.Translate(action.InternalName))) action.Execute(character);
            }

            ImGui.Separator();

            if (ImGui.BeginMenu(this.loc.Translate("Action_AssignGroup"))) {
                bool noGroup = character.CustomGroupId == null;
                if (ImGui.MenuItem(this.loc.Translate("Group_None"), "", noGroup)) {
                    character.CustomGroupId = null;
                    this.registry.SaveMasterList();
                }

                var groups = this.groupRepository.GetGroups();
                if (groups.Count > 0) {
                    ImGui.Separator();
                    foreach (var group in groups) {
                        bool isGroupSelected = character.CustomGroupId == group.Id;
                        if (ImGui.MenuItem(group.Title, "", isGroupSelected)) {
                            character.CustomGroupId = group.Id;
                            this.registry.SaveMasterList();
                        }
                    }
                }
                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu(this.loc.Translate("Action_AssignTag"))) {
                var tags = this.tagRepository.GetTags();
                if (tags.Count == 0) {
                    ImGui.MenuItem(this.loc.Translate("Tag_NoTagsCreated"), "", false, false);
                }
                else {
                    foreach (var tag in tags) {
                        bool hasTag = character.Tags.Contains(tag.Id);
                        if (ImGui.MenuItem(tag.Name, "", hasTag)) {
                            if (hasTag) character.Tags.Remove(tag.Id);
                            else character.Tags.Add(tag.Id);
                            this.registry.SaveMasterList();
                        }
                    }
                }
                ImGui.EndMenu();
            }
            ImGui.EndPopup();
        }
    }

    private void DrawNameCell(Character character, ThemePalette palette, float textOffsetY, bool isDeleted, bool isNearby) {
        ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);

        if (isNearby) {
            ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
            ImGui.TextColored(palette.StatusFallbackOnline, ((char)FontAwesomeIcon.StreetView).ToString());
            ImGui.PopFont();

            var mapId = this.gameDataService.GetMapIdForTerritory(character.LocationId);
            int rawX = (int)(character.LastKnownPosition.X * 1000f);
            int rawY = (int)(character.LastKnownPosition.Z * 1000f);

            var payload = new Dalamud.Game.Text.SeStringHandling.Payloads.MapLinkPayload(character.LocationId, mapId, rawX, rawY);

            if (ImGui.IsItemHovered()) {
                string tooltip = $"{this.loc.Translate("Tooltip_Nearby")}\n{payload.PlaceName} (X: {payload.XCoord:F1}, Y: {payload.YCoord:F1})";

                var spatialRelation = this.proximityService.GetSpatialRelation(character.LastKnownPosition);
                if (spatialRelation.HasValue) {
                    string directionStr = this.loc.Translate(spatialRelation.Value.CardinalDirectionKey);
                    string formatStr = this.loc.Translate("Tooltip_ProximityFormat");

                    if (string.IsNullOrEmpty(formatStr) || formatStr == "Tooltip_ProximityFormat") {
                        formatStr = "{0}y • {1}";
                    }

                    string spatialInfo = string.Format(formatStr, spatialRelation.Value.Distance.ToString("F1"), directionStr);
                    tooltip += $"\n{spatialInfo}";
                }
                ImGui.SetTooltip(tooltip);
            }

            if (ImGui.IsItemClicked()) {
                this.gameGui.OpenMapWithMapLink(payload);
            }
            ImGui.SameLine();
        }

        ImGui.Text(isDeleted ? this.loc.Translate("Profile_DeletedCharacter") : character.Name);

        if (character.IsTrackedForNotifications) {
            ImGui.SameLine();
            ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
            ImGui.TextColored(palette.IconDefaultTint, ((char)FontAwesomeIcon.Bell).ToString());
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.loc.Translate("Tooltip_Tracked"));
        }

        if (!string.IsNullOrWhiteSpace(character.Notes)) {
            ImGui.SameLine();
            ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
            ImGui.TextDisabled(((char)FontAwesomeIcon.StickyNote).ToString());
            ImGui.PopFont();
            if (ImGui.IsItemHovered()) ImGui.SetTooltip(character.Notes);
        }
    }

    private void DrawJobCell(Character character, ThemePalette palette, float textOffsetY) {
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
                    if (ImGui.IsItemHovered()) ImGui.SetTooltip(jobAbbr);
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
    }

    private void DrawGrandCompanyCell(Character character, ThemePalette palette, float textOffsetY) {
        float colWidth = ImGui.GetColumnWidth();

        if (character.GrandCompany > 0) {
            var gcIconId = this.gameDataService.GetGrandCompanyIconId(character.GrandCompany);
            bool iconDrawn = false;

            if (gcIconId > 0) {
                var gcIconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = gcIconId };
                var gcIconWrap = this.textureProvider.GetFromGameIcon(gcIconLookup).GetWrapOrDefault();

                if (gcIconWrap != null) {
                    ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (colWidth - 24.0f) * 0.5f));
                    ImGui.Image(gcIconWrap.Handle, new Vector2(24, 24), Vector2.Zero, Vector2.One, character.IsOnline ? palette.IconDefaultTint : palette.IconDimmedTint);

                    if (ImGui.IsItemHovered()) {
                        ImGui.SetTooltip(this.gameDataService.GetGrandCompanyName(character.GrandCompany));
                    }
                    iconDrawn = true;
                }
            }

            if (!iconDrawn) {
                ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);
                ImGui.Text(this.gameDataService.GetGrandCompanyName(character.GrandCompany));
            }
        }
        else {
            ImGui.Text(string.Empty);
        }
    }

    public virtual void Dispose() {
        this.registry.RegistryUpdated -= this.OnRegistryUpdated;
        this.configurationService.ConfigSaved -= this.OnConfigSaved;
    }
}