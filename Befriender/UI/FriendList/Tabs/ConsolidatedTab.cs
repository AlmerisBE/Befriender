namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Contracts;
using Befriender.UI.FriendList.Components;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using System;
using System.Linq;
using System.Numerics;

public class ConsolidatedTab : ITab {
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private IGameDataService gameDataService;
    private IThemeService themeService;
    private ITextureProvider textureProvider;
    private IProximityService proximityService;
    private ListToolbarComponent toolbarComponent;
    private CharacterProfilePanelComponent profilePanelComponent;

    private string searchQuery = string.Empty;
    private bool showOnlineOnly = false;
    private bool showNearbyOnly = false;
    private bool dummyGroupByGroups = false;

    private Character? selectedCharacter = null;
    private const float PanelWidth = 300f;

    public string InternalName => "Tab_Consolidated";
    public string Name => this.loc.Translate("Tab_Consolidated");
    public bool IsProfilePanelOpen => this.selectedCharacter != null;

    public ConsolidatedTab(
        ICharacterRegistry registry,
        ILocalizationService loc,
        IGameDataService gameDataService,
        IThemeService themeService,
        ITextureProvider textureProvider,
        IProximityService proximityService,
        ListToolbarComponent toolbarComponent,
        CharacterProfilePanelComponent profilePanelComponent) {

        this.registry = registry;
        this.loc = loc;
        this.gameDataService = gameDataService;
        this.themeService = themeService;
        this.textureProvider = textureProvider;
        this.proximityService = proximityService;
        this.toolbarComponent = toolbarComponent;
        this.profilePanelComponent = profilePanelComponent;
    }

    public void Draw() {
        var palette = this.themeService.CurrentPalette;

        this.toolbarComponent.Draw(ref this.showOnlineOnly, ref this.showNearbyOnly, ref this.dummyGroupByGroups, ref this.searchQuery, true);

        ImGui.Separator();
        ImGui.Spacing();

        // Retrieve absolutely all characters from the Master List
        var allCharacters = this.registry.GetAllCharacters();

        if (this.showOnlineOnly) {
            allCharacters = allCharacters.Where(c => c.IsOnline).ToList();
        }

        if (this.showNearbyOnly) {
            allCharacters = allCharacters.Where(c => this.proximityService.IsFriendNearby(c.ContentId)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(this.searchQuery)) {
            allCharacters = allCharacters.Where(c => c.Name.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var charactersList = allCharacters
            .OrderByDescending(c => c.IsActivelyTracked) // Untracked (archived) at the bottom
            .ThenByDescending(c => c.IsOnline)
            .ThenBy(c => c.Name)
            .ToList();

        if (charactersList.Count == 0) {
            ImGui.TextDisabled(this.loc.Translate("Consolidated_Empty"));
            return;
        }

        float tableWidth = this.selectedCharacter != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        if (ImGui.BeginChild("ConsolidatedListContainer", new Vector2(tableWidth, 0))) {
            if (ImGui.BeginTable("ConsolidatedMembersTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY)) {
                ImGui.TableSetupColumn(this.loc.Translate("Column_Status"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(this.loc.Translate("Column_Name"));
                ImGui.TableSetupColumn(this.loc.Translate("Column_Job"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(this.loc.Translate("Column_Location"));
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                float textOffsetY = Math.Max(0, (24.0f - ImGui.GetTextLineHeight()) * 0.5f);

                foreach (var character in charactersList) {
                    ImGui.TableNextRow();

                    bool isAvailable = this.gameDataService.IsFriendAvailable(character.OnlineStateMask);
                    bool isDeleted = string.IsNullOrEmpty(character.Name);
                    Vector4 rowColor;

                    // Specific styling for deleted or untracked (archived) characters
                    if (isDeleted || !character.IsActivelyTracked) {
                        rowColor = palette.TextDimmed;
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

                    // --- COLUMN: STATUS (Contains the invisible Selectable button) ---
                    ImGui.TableNextColumn();
                    float statusColWidth = ImGui.GetColumnWidth();

                    var cursorStart = ImGui.GetCursorPos();
                    bool isSelected = this.selectedCharacter == character;

                    if (ImGui.Selectable($"##row_{character.ContentId}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) {
                        this.selectedCharacter = isSelected ? null : character;
                    }
                    ImGui.SetCursorPos(cursorStart);

                    if (isDeleted || !character.IsActivelyTracked) {
                        // Visual cue for archived/deleted status
                        ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
                        var iconStr = isDeleted ? ((char)Dalamud.Interface.FontAwesomeIcon.Ghost).ToString() : ((char)Dalamud.Interface.FontAwesomeIcon.Archive).ToString();
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
                            Vector4 fallbackColor = character.IsOnline ? palette.StatusFallbackOnline : palette.StatusFallbackOffline;

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

                    if (this.proximityService.IsFriendNearby(character.ContentId)) {
                        ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
                        ImGui.TextColored(palette.StatusFallbackOnline, ((char)Dalamud.Interface.FontAwesomeIcon.StreetView).ToString());
                        ImGui.PopFont();
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(this.loc.Translate("Tooltip_Nearby"));
                        }

                        ImGui.SameLine();
                    }

                    ImGui.Text(isDeleted ? this.loc.Translate("Profile_DeletedCharacter") : character.Name);

                    if (!string.IsNullOrWhiteSpace(character.Notes)) {
                        ImGui.SameLine();
                        ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
                        ImGui.TextDisabled(((char)Dalamud.Interface.FontAwesomeIcon.StickyNote).ToString());
                        ImGui.PopFont();
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(character.Notes);
                        }
                    }

                    // --- COLUMN: JOB ---
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
                                var imageTint = character.IsOnline ? palette.IconDefaultTint : palette.IconDimmedTint;
                                ImGui.Image(jIconWrap.Handle, new Vector2(24, 24), Vector2.Zero, Vector2.One, imageTint);
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

                    // --- COLUMN: LOCATION ---
                    ImGui.TableNextColumn();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);

                    string locationName = this.gameDataService.GetDisplayLocation(character.LocationId, character.CurrentWorldId, character.HomeWorldId, character.OnlineStateMask);
                    ImGui.Text(string.IsNullOrEmpty(locationName) || locationName == "0" ? this.loc.Translate("Profile_Unknown") : locationName);

                    ImGui.PopStyleColor();
                }

                ImGui.EndTable();
            }
        }
        ImGui.EndChild();

        // --- PROFILE PANEL DRAWING ---
        if (this.selectedCharacter != null) {
            ImGui.SameLine();
            this.profilePanelComponent.Draw(PanelWidth, this.selectedCharacter, () => this.selectedCharacter = null);
        }
    }
}