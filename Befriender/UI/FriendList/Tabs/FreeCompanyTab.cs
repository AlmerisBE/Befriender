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
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

public class FreeCompanyTab : ITab {
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private IGameDataService gameDataService;
    private IThemeService themeService;
    private ITextureProvider textureProvider;
    private IProximityService proximityService;
    private ICharacterActionService actionService;
    private ListToolbarComponent toolbarComponent;
    private CharacterProfilePanelComponent profilePanelComponent;

    private Guid fcSourceId;
    private string searchQuery = string.Empty;
    private bool showOnlineOnly = false;
    private bool showNearbyOnly = false;
    private bool dummyGroupByGroups = false;

    private Character? selectedCharacter = null;
    private const float PanelWidth = 300f;

    public string InternalName => "Tab_FreeCompany";
    public string Name => this.loc.Translate("Tab_FreeCompany");
    public bool IsProfilePanelOpen => this.selectedCharacter != null;

    public FreeCompanyTab(
        ICharacterRegistry registry,
        IEnumerable<ICharacterSource> sources,
        ILocalizationService loc,
        IGameDataService gameDataService,
        IThemeService themeService,
        ITextureProvider textureProvider,
        IProximityService proximityService,
        ICharacterActionService actionService,
        ListToolbarComponent toolbarComponent,
        CharacterProfilePanelComponent profilePanelComponent) {

        this.registry = registry;
        this.loc = loc;
        this.gameDataService = gameDataService;
        this.themeService = themeService;
        this.textureProvider = textureProvider;
        this.proximityService = proximityService;
        this.actionService = actionService;
        this.toolbarComponent = toolbarComponent;
        this.profilePanelComponent = profilePanelComponent;

        var fcSource = sources.FirstOrDefault(s => s.Name == "FreeCompany");
        if (fcSource != null) {
            this.fcSourceId = fcSource.SourceId;
        }
    }

    public void Draw() {
        var palette = this.themeService.CurrentPalette;

        this.toolbarComponent.Draw(ref this.showOnlineOnly, ref this.showNearbyOnly, ref this.dummyGroupByGroups, ref this.searchQuery, true);

        ImGui.Separator();
        ImGui.Spacing();

        if (this.fcSourceId == Guid.Empty) {
            ImGui.TextDisabled("FreeCompany source is not registered.");
            return;
        }

        var allMembers = this.registry.GetCharactersBySource(this.fcSourceId);

        if (this.showOnlineOnly) {
            allMembers = allMembers.Where(m => m.IsOnline).ToList();
        }

        if (this.showNearbyOnly) {
            allMembers = allMembers.Where(m => this.proximityService.IsFriendNearby(m.ContentId)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(this.searchQuery)) {
            allMembers = allMembers.Where(m => m.Name.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var membersList = allMembers.OrderByDescending(m => m.IsOnline).ThenBy(m => m.Name).ToList();

        if (membersList.Count == 0) {
            ImGui.TextDisabled(this.loc.Translate("FreeCompany_Empty"));
            return;
        }

        float footerHeight = ImGui.GetFrameHeight() + (ImGui.GetStyle().ItemSpacing.Y * 3);
        float tableWidth = this.selectedCharacter != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        if (ImGui.BeginChild("FcListContainer", new Vector2(tableWidth, -footerHeight))) {
            if (ImGui.BeginTable("FcMembersTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY)) {
                ImGui.TableSetupColumn(this.loc.Translate("Column_Status"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(this.loc.Translate("Column_Name"));
                ImGui.TableSetupColumn(this.loc.Translate("Column_Job"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(this.loc.Translate("Column_Location"));
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                float textOffsetY = Math.Max(0, (24.0f - ImGui.GetTextLineHeight()) * 0.5f);

                foreach (var member in membersList) {
                    ImGui.TableNextRow();

                    bool isAvailable = this.gameDataService.IsFriendAvailable(member.OnlineStateMask);
                    Vector4 rowColor;

                    if (!member.IsOnline) {
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
                    bool isSelected = this.selectedCharacter == member;

                    if (ImGui.Selectable($"##row_{member.ContentId}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) {
                        this.selectedCharacter = isSelected ? null : member;
                    }
                    ImGui.SetCursorPos(cursorStart);

                    // MENU CONTEXTUEL CLIC DROIT
                    if (ImGui.BeginPopupContextItem($"ContextMenu_{member.ContentId}")) {
                        var actions = this.actionService.GetAvailableActions(member);
                        if (actions.Count == 0) {
                            ImGui.MenuItem(this.loc.Translate("Action_NoneAvailable"), false);
                        }

                        foreach (var action in actions) {
                            if (ImGui.MenuItem(this.loc.Translate(action.InternalName))) {
                                action.Execute(member);
                            }
                        }
                        ImGui.EndPopup();
                    }

                    ulong effectiveMask = member.IsOnline ? member.OnlineStateMask : 0;
                    var statusInfo = this.gameDataService.GetOnlineStatusInfo(effectiveMask, member.CurrentWorldId, member.HomeWorldId, member.LocationId);
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
                        Vector4 fallbackColor = member.IsOnline ? palette.StatusFallbackOnline : palette.StatusFallbackOffline;

                        ImGui.PushStyleColor(ImGuiCol.Text, fallbackColor);
                        ImGui.Text("●");
                        ImGui.PopStyleColor();
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(statusInfo.Name);
                        }
                    }

                    ImGui.TableNextColumn();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);

                    if (this.proximityService.IsFriendNearby(member.ContentId)) {
                        ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
                        ImGui.TextColored(palette.StatusFallbackOnline, ((char)Dalamud.Interface.FontAwesomeIcon.StreetView).ToString());
                        ImGui.PopFont();
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(this.loc.Translate("Tooltip_Nearby"));
                        }

                        ImGui.SameLine();
                    }

                    ImGui.Text(member.Name);

                    if (!string.IsNullOrWhiteSpace(member.Notes)) {
                        ImGui.SameLine();
                        ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
                        ImGui.TextDisabled(((char)Dalamud.Interface.FontAwesomeIcon.StickyNote).ToString());
                        ImGui.PopFont();
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(member.Notes);
                        }
                    }

                    ImGui.TableNextColumn();
                    float jobColWidth = ImGui.GetColumnWidth();

                    if (member.JobId > 0) {
                        var jobIconId = this.gameDataService.GetJobIconId(member.JobId);
                        var jobAbbr = this.gameDataService.GetJobAbbreviation(member.JobId);
                        bool iconDrawn = false;

                        if (jobIconId > 0) {
                            var jIconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = jobIconId };
                            var jIconWrap = this.textureProvider.GetFromGameIcon(jIconLookup).GetWrapOrDefault();

                            if (jIconWrap != null) {
                                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (jobColWidth - 24.0f) * 0.5f));
                                var imageTint = member.IsOnline ? palette.IconDefaultTint : palette.IconDimmedTint;
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

                    ImGui.TableNextColumn();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);

                    string locationName = this.gameDataService.GetDisplayLocation(member.LocationId, member.CurrentWorldId, member.HomeWorldId, member.OnlineStateMask);
                    ImGui.Text(string.IsNullOrEmpty(locationName) || locationName == "0" ? this.loc.Translate("Profile_Unknown") : locationName);

                    ImGui.PopStyleColor();
                }

                ImGui.EndTable();
            }
        }
        ImGui.EndChild();

        if (this.selectedCharacter != null) {
            ImGui.SameLine();
            this.profilePanelComponent.Draw(PanelWidth, -footerHeight, this.selectedCharacter, () => this.selectedCharacter = null);
        }
    }
}