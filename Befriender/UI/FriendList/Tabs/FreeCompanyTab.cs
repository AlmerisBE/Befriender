namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.FreeCompany.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
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

public class FreeCompanyTab : ITab {
    private ICharacterRegistry registry;
    private IFreeCompanyRepository fcRepository;
    private IFriendRepository friendRepository;
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

    public string InternalName => "Tab_FreeCompany";
    public string Name => this.loc.Translate("Tab_FreeCompany");
    public bool IsProfilePanelOpen => this.selectedCharacter != null;

    public FreeCompanyTab(
        ICharacterRegistry registry,
        IFreeCompanyRepository fcRepository,
        IFriendRepository friendRepository,
        ILocalizationService loc,
        IGameDataService gameDataService,
        IThemeService themeService,
        ITextureProvider textureProvider,
        IProximityService proximityService,
        ListToolbarComponent toolbarComponent,
        CharacterProfilePanelComponent profilePanelComponent) {

        this.registry = registry;
        this.fcRepository = fcRepository;
        this.friendRepository = friendRepository;
        this.loc = loc;
        this.gameDataService = gameDataService;
        this.themeService = themeService;
        this.textureProvider = textureProvider;
        this.proximityService = proximityService;
        this.toolbarComponent = toolbarComponent;
        this.profilePanelComponent = profilePanelComponent;
    }

    public void Draw() {
        var fcSourceId = this.fcRepository.SourceId;
        var palette = this.themeService.CurrentPalette;

        this.toolbarComponent.Draw(ref this.showOnlineOnly, ref this.showNearbyOnly, ref this.dummyGroupByGroups, ref this.searchQuery, true);

        ImGui.Separator();
        ImGui.Spacing();

        var allMembers = this.registry.GetConsolidatedCharacters()
            .Where(c => c.ActiveSourceIds.Contains(fcSourceId));

        if (this.showOnlineOnly) {
            allMembers = allMembers.Where(m => m.IsOnline);
        }

        if (this.showNearbyOnly) {
            allMembers = allMembers.Where(m => this.proximityService.IsFriendNearby(m.ContentId));
        }

        if (!string.IsNullOrWhiteSpace(this.searchQuery)) {
            allMembers = allMembers.Where(m => m.Name.Contains(this.searchQuery, StringComparison.OrdinalIgnoreCase));
        }

        var membersList = allMembers.OrderByDescending(m => m.IsOnline).ThenBy(m => m.Name).ToList();

        if (membersList.Count == 0) {
            ImGui.TextDisabled(this.loc.Translate("FreeCompany_Empty"));
            return;
        }

        // Calculate available width for the table, reserving space for the profile panel if it's open
        float tableWidth = this.selectedCharacter != null ? ImGui.GetContentRegionAvail().X - PanelWidth - ImGui.GetStyle().ItemSpacing.X : 0f;

        // Wrap the table in a child to allow the side panel to sit next to it
        if (ImGui.BeginChild("FcListContainer", new Vector2(tableWidth, 0))) {
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

                    // --- COLUMN: STATUS (Contains the invisible Selectable button) ---
                    ImGui.TableNextColumn();
                    float statusColWidth = ImGui.GetColumnWidth();

                    var cursorStart = ImGui.GetCursorPos();
                    bool isSelected = this.selectedCharacter == member;

                    if (ImGui.Selectable($"##row_{member.ContentId}", isSelected, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowItemOverlap, new Vector2(0, 24))) {
                        this.selectedCharacter = isSelected ? null : member;
                    }
                    ImGui.SetCursorPos(cursorStart);

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

                    // --- COLUMN: NAME ---
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

                    // --- COLUMN: JOB ---
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

                    // --- COLUMN: LOCATION ---
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

        // --- PROFILE PANEL DRAWING ---
        if (this.selectedCharacter != null) {
            ImGui.SameLine();

            // Bridge: If the FC member is also a friend, use their real profile. Otherwise, project a dummy profile.
            var profileToDisplay = this.friendRepository.GetFriends().FirstOrDefault(f => f.ContentId == this.selectedCharacter.ContentId)
                                   ?? this.MapToFriendProfile(this.selectedCharacter);

            this.profilePanelComponent.Draw(PanelWidth, profileToDisplay, () => this.selectedCharacter = null);
        }
    }

    private FriendProfile MapToFriendProfile(Character c) {
        return new FriendProfile {
            Id = c.Id,
            ContentId = c.ContentId,
            Name = c.Name,
            HomeWorldId = c.HomeWorldId,
            CurrentWorldId = c.CurrentWorldId,
            JobId = c.JobId,
            Level = c.Level,
            LocationId = c.LocationId,
            IsOnline = c.IsOnline,
            FcTag = c.FcTag,
            OnlineStateMask = c.OnlineStateMask,
            OnlineStatusId = c.OnlineStatusId,
            ClientLanguages = c.ClientLanguages,
            TitleId = c.TitleId,
            Race = c.Race,
            Tribe = c.Tribe,
            Gender = c.Gender,
            IsFantasiaDetected = c.IsFantasiaDetected,
            AddedAt = c.AddedAt,
            AddedLocationId = c.AddedLocationId,
            LastSeenAt = c.LastSeenAt,
            ArchivedAt = c.ArchivedAt,
            CustomGroupId = c.CustomGroupId,
            Tags = c.Tags.ToList(),
            PreviousNames = c.PreviousNames.ToList(),
            Notes = c.Notes,
            IsArchived = c.IsArchived,
            IsCharacterDeleted = c.IsCharacterDeleted,
            IsMarkedForRemoval = c.IsMarkedForRemoval,
            IsMissing = c.IsMissing,
            GrandCompany = c.GrandCompany,
            IsTrackedForNotifications = c.IsTrackedForNotifications
        };
    }
}