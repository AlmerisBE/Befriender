namespace Befriender.UI.MainWindow.Components;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.Theme.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using Dalamud.Plugin.Services;
using System;
using System.Linq;
using System.Numerics;

public class CharacterProfilePanelComponent {
    private IGameDataService gameDataService;
    private ICharacterRegistry characterRegistry;
    private ILocalizationService loc;
    private ICharacterActionService actionService;
    private ITextureProvider textureProvider;
    private ICharacterGroupRepository groupRepository;
    private ICharacterTagRepository tagRepository;
    private IThemeService themeService;

    private string notesBuffer = string.Empty;
    private ulong currentCharacterId = 0;

    public CharacterProfilePanelComponent(
        IGameDataService gameDataService,
        ICharacterRegistry characterRegistry,
        ILocalizationService loc,
        ICharacterActionService actionService,
        ITextureProvider textureProvider,
        ICharacterGroupRepository groupRepository,
        ICharacterTagRepository tagRepository,
        IThemeService themeService) {

        this.gameDataService = gameDataService;
        this.characterRegistry = characterRegistry;
        this.loc = loc;
        this.actionService = actionService;
        this.textureProvider = textureProvider;
        this.groupRepository = groupRepository;
        this.tagRepository = tagRepository;
        this.themeService = themeService;
    }

    public void Draw(float panelWidth, float panelHeight, Character character, Action onClose) {
        if (ImGui.BeginChild("ProfilePanel", new Vector2(panelWidth, panelHeight), true)) {
            if (this.currentCharacterId != character.ContentId) {
                this.currentCharacterId = character.ContentId;
                this.notesBuffer = character.Notes ?? string.Empty;
            }

            bool isDeleted = string.IsNullOrEmpty(character.Name);
            ImGui.TextUnformatted(isDeleted ? this.loc.Translate("Profile_DeletedCharacter") : character.Name);

            float closeButtonSize = ImGui.GetFrameHeight();
            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - closeButtonSize);

            int closeBtnId = unchecked("ClosePanel".GetHashCode() ^ character.ContentId.GetHashCode());
            if (ImGuiComponents.IconButton(closeBtnId, FontAwesomeIcon.Times)) onClose();

            ImGui.Separator();

            if (ImGui.BeginChild("ProfileScrollArea", new Vector2(0, 0), false)) {
                ImGui.Spacing();

                var actions = this.actionService.GetAvailableActions(character);
                if (actions.Count > 0) {
                    var style = ImGui.GetStyle();
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

                    for (int i = 0; i < actions.Count; i++) {
                        var action = actions[i];
                        int buttonId = unchecked((int)action.Icon ^ character.ContentId.GetHashCode());

                        if (ImGuiComponents.IconButton(buttonId, action.Icon)) action.Execute(character);
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.loc.Translate(action.InternalName));

                        if (i + 1 < actions.Count) {
                            float lastItemX2 = ImGui.GetItemRectMax().X;
                            float nextItemX2 = lastItemX2 + style.ItemSpacing.X + ImGui.GetItemRectSize().X;
                            if (nextItemX2 < windowVisibleX2) ImGui.SameLine();
                        }
                    }
                    ImGui.Spacing();
                }

                var groups = this.groupRepository.GetGroups().ToList();
                var groupNames = groups.Select(g => g.Title).ToList();
                groupNames.Insert(0, this.loc.Translate("Group_None"));

                int currentIndex = 0;
                if (character.CustomGroupId.HasValue) {
                    var idx = groups.FindIndex(g => g.Id == character.CustomGroupId.Value);
                    if (idx >= 0) currentIndex = idx + 1;
                }

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGui.Combo($"##groupSelect_{character.ContentId}", ref currentIndex, groupNames.ToArray(), groupNames.Count)) {
                    character.CustomGroupId = currentIndex == 0 ? null : groups[currentIndex - 1].Id;
                    this.characterRegistry.SaveMasterList();
                }

                ImGui.Spacing();
                var allTags = this.tagRepository.GetTags();
                var assignedTags = allTags.Where(t => character.Tags.Contains(t.Id)).ToList();
                var unassignedTags = allTags.Where(t => !character.Tags.Contains(t.Id)).ToList();

                ImGui.TextUnformatted(this.loc.Translate("Profile_Tags"));

                if (assignedTags.Count > 0) {
                    float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
                    var style = ImGui.GetStyle();

                    for (int i = 0; i < assignedTags.Count; i++) {
                        var tag = assignedTags[i];
                        ImGui.BeginGroup();
                        ImGui.TextDisabled(tag.Name);
                        ImGui.SameLine(0, 2f);

                        if (ImGuiComponents.IconButton(unchecked(tag.Id.GetHashCode() ^ character.ContentId.GetHashCode()), FontAwesomeIcon.Times)) {
                            character.Tags.Remove(tag.Id);
                            this.characterRegistry.SaveMasterList();
                        }
                        ImGui.EndGroup();

                        if (i + 1 < assignedTags.Count) {
                            float nextItemX2 = ImGui.GetItemRectMax().X + style.ItemSpacing.X + ImGui.CalcTextSize(assignedTags[i + 1].Name).X + ImGui.GetFrameHeight();
                            if (nextItemX2 < windowVisibleX2) ImGui.SameLine();
                        }
                    }
                }

                if (unassignedTags.Count > 0) {
                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                    if (ImGui.BeginCombo($"##assignExistingTag_{character.ContentId}", this.loc.Translate("Profile_SelectTags"))) {
                        foreach (var tag in unassignedTags) {
                            if (ImGui.Selectable(tag.Name)) {
                                character.Tags.Add(tag.Id);
                                this.characterRegistry.SaveMasterList();
                            }
                        }
                        ImGui.EndCombo();
                    }
                }
                else if (allTags.Count == 0) ImGui.TextDisabled(this.loc.Translate("Tag_NoTagsCreated"));

                ImGui.Spacing();

                if (ImGui.CollapsingHeader(this.loc.Translate("Section_Status"), ImGuiTreeNodeFlags.DefaultOpen)) {
                    ImGui.Text($"{this.loc.Translate("Column_Status")}: ");
                    ImGui.SameLine();

                    var statusInfo = this.gameDataService.GetOnlineStatusInfo(character.IsOnline, character.OnlineStateMask, character.OnlineStatusId, character.CurrentWorldId, character.HomeWorldId, character.LocationId);

                    var statusIconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = statusInfo.IconId };
                    var statusIconWrap = this.textureProvider.GetFromGameIcon(statusIconLookup).GetWrapOrDefault();

                    if (statusIconWrap != null) {
                        float iconSize = ImGui.GetTextLineHeight();
                        float currentY = ImGui.GetCursorPosY();
                        ImGui.Image(statusIconWrap.Handle, new Vector2(iconSize, iconSize));
                        ImGui.SameLine(0, 4f);
                        ImGui.SetCursorPosY(currentY);
                    }
                    ImGui.Text(statusInfo.Name);

                    string displayLocation = this.gameDataService.GetDisplayLocation(character.LocationId, character.CurrentWorldId, character.HomeWorldId, character.OnlineStateMask);
                    if (string.IsNullOrEmpty(displayLocation) || displayLocation == "0") displayLocation = this.loc.Translate("Profile_Unknown");

                    ImGui.Text($"{this.loc.Translate("Column_Location")}: {displayLocation}");

                    string lastSeenStr = character.IsOnline ? this.loc.Translate("Profile_Online") : (character.LastSeenAt == DateTime.MinValue ? this.loc.Translate("Profile_Unknown") : this.loc.Translate("Profile_DaysAgo", (int)(DateTime.Now - character.LastSeenAt).TotalDays));

                    if (!character.IsOnline && character.LastSeenAt != DateTime.MinValue) {
                        var diff = DateTime.Now - character.LastSeenAt;
                        if (diff.TotalDays < 1 && diff.TotalHours >= 1) lastSeenStr = this.loc.Translate("Profile_HoursAgo", (int)diff.TotalHours);
                        else if (diff.TotalHours < 1) lastSeenStr = this.loc.Translate("Profile_MinsAgo", (int)diff.TotalMinutes);
                    }

                    ImGui.Text($"{this.loc.Translate("Profile_LastSeen")}: {lastSeenStr}");
                    ImGui.Spacing();
                }

                if (ImGui.CollapsingHeader(this.loc.Translate("Section_Character"), ImGuiTreeNodeFlags.DefaultOpen)) {
                    ImGui.Text($"{this.loc.Translate("Profile_Job")}: ");
                    ImGui.SameLine();

                    if (character.JobId > 0) {
                        var jobIconId = this.gameDataService.GetJobIconId(character.JobId);
                        if (jobIconId > 0) {
                            var jobIconLookup = new Dalamud.Interface.Textures.GameIconLookup { IconId = jobIconId };
                            var jobIconWrap = this.textureProvider.GetFromGameIcon(jobIconLookup).GetWrapOrDefault();

                            if (jobIconWrap != null) {
                                float iconSize = ImGui.GetTextLineHeight();
                                float currentY = ImGui.GetCursorPosY();
                                ImGui.Image(jobIconWrap.Handle, new Vector2(iconSize, iconSize));
                                ImGui.SameLine(0, 4f);
                                ImGui.SetCursorPosY(currentY);
                            }
                        }
                    }
                    var jobAbbr = character.JobId > 0 ? this.gameDataService.GetJobAbbreviation(character.JobId) : this.loc.Translate("Profile_None");
                    ImGui.Text(jobAbbr);

                    if (character.Level > 0) ImGui.Text($"{this.loc.Translate("Profile_Level")}: {character.Level}");

                    string title = this.gameDataService.GetTitleName(character.TitleId, character.Gender);
                    if (!string.IsNullOrEmpty(title)) ImGui.Text($"{this.loc.Translate("Profile_Title")}: {title}");

                    string race = this.gameDataService.GetRaceName(character.Race, character.Gender);
                    string tribe = this.gameDataService.GetTribeName(character.Tribe, character.Gender);
                    if (!string.IsNullOrEmpty(race)) ImGui.Text($"{this.loc.Translate("Profile_Race")}: {race} ({tribe})");

                    if (character.IsFantasiaDetected) {
                        ImGui.PushStyleColor(ImGuiCol.Text, this.themeService.CurrentPalette.TextMarkedForRemoval);
                        ImGui.Text(this.loc.Translate("Profile_FantasiaDetected"));
                        ImGui.PopStyleColor();
                        ImGui.SameLine();

                        if (ImGuiComponents.IconButton(FontAwesomeIcon.CheckDouble)) {
                            character.IsFantasiaDetected = false;
                            this.characterRegistry.SaveMasterList();
                        }
                        if (ImGui.IsItemHovered()) ImGui.SetTooltip(this.loc.Translate("Action_ClearFantasia"));
                    }

                    string fcName = string.IsNullOrEmpty(character.FcTag) ? this.loc.Translate("Profile_None") : character.FcTag;
                    ImGui.Text($"{this.loc.Translate("Profile_FC")}: {fcName}");

                    ImGui.Text($"{this.loc.Translate("Profile_GrandCompany")}: ");
                    ImGui.SameLine();
                    var gcIconId = this.gameDataService.GetGrandCompanyIconId(character.GrandCompany);
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

                    string gcName = character.GrandCompany > 0 ? this.gameDataService.GetGrandCompanyName(character.GrandCompany) : this.loc.Translate("Profile_None");
                    ImGui.Text(gcName);
                    ImGui.Text($"{this.loc.Translate("Profile_HomeWorld")}: {this.gameDataService.GetWorldName(character.HomeWorldId)}");
                    ImGui.Text($"{this.loc.Translate("Profile_ClientLanguages")}: {this.gameDataService.GetClientLanguageString(character.ClientLanguages)}");
                    ImGui.Spacing();
                }

                if (ImGui.CollapsingHeader(this.loc.Translate("Section_System"))) {
                    var dateStr = character.AddedAt == DateTime.MinValue ? this.loc.Translate("Profile_Unknown") : character.AddedAt.ToShortDateString();
                    var locStr = this.gameDataService.GetLocationName(character.AddedLocationId);
                    ImGui.Text($"{this.loc.Translate("Profile_Added")}: {dateStr}");
                    ImGui.Text($"{this.loc.Translate("Profile_MetAt")}: {locStr}");

                    string listStatus;
                    if (isDeleted) listStatus = this.loc.Translate("Profile_StatusDeleted");
                    else if (!character.IsActivelyTracked) listStatus = this.loc.Translate("Profile_StatusUnsynchronized");
                    else listStatus = this.loc.Translate("Profile_StatusActive");

                    ImGui.Text($"{this.loc.Translate("Profile_ListStatus")}: {listStatus}");

                    if (character.PreviousNames != null && character.PreviousNames.Count > 0) {
                        ImGui.Spacing();
                        ImGui.TextUnformatted(this.loc.Translate("Section_NameHistory"));
                        foreach (var oldName in character.PreviousNames) ImGui.BulletText(oldName);
                    }
                    ImGui.Spacing();
                }

                if (ImGui.CollapsingHeader(this.loc.Translate("Section_Notes"), ImGuiTreeNodeFlags.DefaultOpen)) {
                    ImGui.InputTextMultiline("##notes", ref this.notesBuffer, 2048, new Vector2(-1, 100));
                    if (ImGui.IsItemDeactivatedAfterEdit()) {
                        character.Notes = this.notesBuffer;
                        this.characterRegistry.SaveMasterList();
                    }
                    ImGui.Spacing();
                }
            }
            ImGui.EndChild();
        }
        ImGui.EndChild();
    }
}