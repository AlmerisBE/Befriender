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

public class ListTab : ITab {
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private IGameDataService gameDataService;
    private IThemeService themeService;
    private ITextureProvider textureProvider;
    private IProximityService proximityService;
    private ICharacterActionService actionService;
    private ListToolbarComponent toolbarComponent;
    private CharacterProfilePanelComponent profilePanelComponent;

    private Guid friendSourceId;
    private string searchQuery = string.Empty;
    private bool showOnlineOnly = false;
    private bool showNearbyOnly = false;
    private bool groupByGroups = true;

    private Character? selectedCharacter = null;
    private const float PanelWidth = 300f;

    public string InternalName => "Tab_List";
    public string Name => this.loc.Translate("Tab_List");
    public bool IsProfilePanelOpen => this.selectedCharacter != null;

    public ListTab(
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

        var friendSource = sources.FirstOrDefault(s => s.Name == "FriendList");
        if (friendSource != null) {
            this.friendSourceId = friendSource.SourceId;
        }
    }

    public void Draw() {
        var palette = this.themeService.CurrentPalette;

        // ACTIVATION DU FILTRE EN LIGNE (dernier argument = true)
        this.toolbarComponent.Draw(ref this.showOnlineOnly, ref this.showNearbyOnly, ref this.groupByGroups, ref this.searchQuery, true);

        ImGui.Separator();
        ImGui.Spacing();

        if (this.friendSourceId == Guid.Empty) {
            ImGui.TextDisabled("FriendList source is not registered.");
            return;
        }

        var allFriends = this.registry.GetCharactersBySource(this.friendSourceId);

        if (this.showOnlineOnly) {
            allFriends = allFriends.Where(m => m.IsOnline).ToList();
        }

        if (this.showNearbyOnly) {
            allFriends = allFriends.Where(m => this.proximityService.IsFriendNearby(m.ContentId)).ToList();
        }

        if (!string.IsNullOrWhiteSpace(this.searchQuery)) {
            allFriends = allFriends.Where(m => m.Name.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var friendsList = allFriends.OrderByDescending(m => m.IsOnline).ThenBy(m => m.Name).ToList();

        if (friendsList.Count == 0) {
            ImGui.TextDisabled(this.loc.Translate("List_Empty"));
            return;
        }

        float footerHeight = ImGui.GetFrameHeight() + (ImGui.GetStyle().ItemSpacing.Y * 3);
        float tableWidth = this.selectedCharacter != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        if (ImGui.BeginChild("FriendListContainer", new Vector2(tableWidth, -footerHeight))) {
            if (ImGui.BeginTable("FriendMembersTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY)) {
                ImGui.TableSetupColumn(this.loc.Translate("Column_Status"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(this.loc.Translate("Column_Name"));
                ImGui.TableSetupColumn(this.loc.Translate("Column_Job"), ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn(this.loc.Translate("Column_Location"));
                ImGui.TableSetupScrollFreeze(0, 1);
                ImGui.TableHeadersRow();

                float textOffsetY = Math.Max(0, (24.0f - ImGui.GetTextLineHeight()) * 0.5f);

                foreach (var friend in friendsList) {
                    ImGui.TableNextRow();

                    bool isAvailable = this.gameDataService.IsFriendAvailable(friend.OnlineStateMask);
                    Vector4 rowColor;

                    if (!friend.IsOnline) {
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
                    bool isSelected = this.selectedCharacter == friend;

                    if (ImGui.Selectable($"##row_{friend.ContentId}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) {
                        this.selectedCharacter = isSelected ? null : friend;
                    }
                    ImGui.SetCursorPos(cursorStart);

                    // MENU CONTEXTUEL CLIC DROIT
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

                    ImGui.TableNextColumn();
                    ImGui.SetCursorPosY(ImGui.GetCursorPosY() + textOffsetY);

                    if (this.proximityService.IsFriendNearby(friend.ContentId)) {
                        ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
                        ImGui.TextColored(palette.StatusFallbackOnline, ((char)Dalamud.Interface.FontAwesomeIcon.StreetView).ToString());
                        ImGui.PopFont();
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(this.loc.Translate("Tooltip_Nearby"));
                        }

                        ImGui.SameLine();
                    }

                    ImGui.Text(friend.Name);

                    if (!string.IsNullOrWhiteSpace(friend.Notes)) {
                        ImGui.SameLine();
                        ImGui.PushFont(Dalamud.Interface.UiBuilder.IconFont);
                        ImGui.TextDisabled(((char)Dalamud.Interface.FontAwesomeIcon.StickyNote).ToString());
                        ImGui.PopFont();
                        if (ImGui.IsItemHovered()) {
                            ImGui.SetTooltip(friend.Notes);
                        }
                    }

                    ImGui.TableNextColumn();
                    float jobColWidth = ImGui.GetColumnWidth();

                    if (friend.JobId > 0) {
                        var jobIconId = this.gameDataService.GetJobIconId(friend.JobId);
                        var jobAbbr = this.gameDataService.GetJobAbbreviation(friend.JobId);
                        bool iconDrawn = false;

                        if (jobIconId > 0) {
                            var jIconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = jobIconId };
                            var jIconWrap = this.textureProvider.GetFromGameIcon(jIconLookup).GetWrapOrDefault();

                            if (jIconWrap != null) {
                                ImGui.SetCursorPosX(ImGui.GetCursorPosX() + Math.Max(0, (jobColWidth - 24.0f) * 0.5f));
                                var imageTint = friend.IsOnline ? palette.IconDefaultTint : palette.IconDimmedTint;
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

                    string locationName = this.gameDataService.GetDisplayLocation(friend.LocationId, friend.CurrentWorldId, friend.HomeWorldId, friend.OnlineStateMask);
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