namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
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

public class FriendProfilePanelComponent {
    private IGameDataService gameDataService;
    private IFriendRepository friendRepository;
    private ILocalizationService loc;
    private IFriendActionService actionService;
    private ITextureProvider textureProvider;
    private IFriendGroupRepository groupRepository;
    private IFriendTagRepository tagRepository;
    private IThemeService themeService;

    private string notesBuffer = string.Empty;
    private ulong currentFriendId = 0;

    public FriendProfilePanelComponent(IGameDataService gameDataService, IFriendRepository friendRepository, ILocalizationService loc, IFriendActionService actionService, ITextureProvider textureProvider, IFriendGroupRepository groupRepository, IFriendTagRepository tagRepository, IThemeService themeService) {
        this.gameDataService = gameDataService;
        this.friendRepository = friendRepository;
        this.loc = loc;
        this.actionService = actionService;
        this.textureProvider = textureProvider;
        this.groupRepository = groupRepository;
        this.tagRepository = tagRepository;
        this.themeService = themeService;
    }

    public void Draw(float panelWidth, FriendProfile friend, Action onClose) {
        if (ImGui.BeginChild("ProfilePanel", new Vector2(panelWidth, 0), true)) {
            if (this.currentFriendId != friend.ContentId) {
                this.currentFriendId = friend.ContentId;
                this.notesBuffer = friend.Notes ?? string.Empty;
            }

            ImGui.TextUnformatted(string.IsNullOrEmpty(friend.Name) ? this.loc.Translate("Profile_DeletedCharacter") : friend.Name);

            float closeButtonSize = ImGui.GetFrameHeight();
            ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - closeButtonSize);

            int closeBtnId = unchecked("ClosePanel".GetHashCode() ^ friend.ContentId.GetHashCode());
            if (ImGuiComponents.IconButton(closeBtnId, FontAwesomeIcon.Times)) {
                onClose();
            }

            ImGui.Separator();
            ImGui.Spacing();

            // --- Actions ---
            var actions = this.actionService.GetAvailableActions(friend);
            if (actions.Count > 0) {
                var style = ImGui.GetStyle();
                float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;

                for (int i = 0; i < actions.Count; i++) {
                    var action = actions[i];
                    int buttonId = unchecked((int)action.Icon ^ friend.ContentId.GetHashCode());

                    if (ImGuiComponents.IconButton(buttonId, action.Icon)) {
                        action.Execute(friend);
                    }

                    if (ImGui.IsItemHovered()) {
                        ImGui.SetTooltip(this.loc.Translate(action.InternalName));
                    }

                    if (i + 1 < actions.Count) {
                        float lastItemX2 = ImGui.GetItemRectMax().X;
                        float nextItemX2 = lastItemX2 + style.ItemSpacing.X + ImGui.GetItemRectSize().X;
                        if (nextItemX2 < windowVisibleX2) {
                            ImGui.SameLine();
                        }
                    }
                }
                ImGui.Spacing();
            }

            // --- Categorization ---
            var groups = this.groupRepository.GetGroups().ToList();
            var groupNames = groups.Select(g => g.Title).ToList();
            groupNames.Insert(0, this.loc.Translate("Group_None"));

            int currentIndex = 0;
            if (friend.CustomGroupId.HasValue) {
                var idx = groups.FindIndex(g => g.Id == friend.CustomGroupId.Value);
                if (idx >= 0) {
                    currentIndex = idx + 1;
                }
            }

            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
            if (ImGui.Combo($"##groupSelect_{friend.ContentId}", ref currentIndex, groupNames.ToArray(), groupNames.Count)) {
                friend.CustomGroupId = currentIndex == 0 ? null : groups[currentIndex - 1].Id;
                this.friendRepository.Save();
            }

            var allTags = this.tagRepository.GetTags();
            if (allTags.Count > 0) {
                var assignedTags = allTags.Where(t => friend.Tags.Contains(t.Id)).ToList();
                string preview = assignedTags.Count > 0 ? string.Join(", ", assignedTags.Select(t => t.Name)) : this.loc.Translate("Profile_SelectTags");

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);
                if (ImGui.BeginCombo($"##tagSelect_{friend.ContentId}", preview)) {
                    bool tagsChanged = false;

                    foreach (var tag in allTags) {
                        bool isSelected = friend.Tags.Contains(tag.Id);
                        if (ImGui.Checkbox($"{tag.Name}##{tag.Id}", ref isSelected)) {
                            if (isSelected) {
                                friend.Tags.Add(tag.Id);
                            }
                            else {
                                friend.Tags.Remove(tag.Id);
                            }

                            tagsChanged = true;
                        }
                    }

                    if (tagsChanged) {
                        this.friendRepository.Save();
                    }

                    ImGui.EndCombo();
                }
            }

            ImGui.Spacing();

            // --- Accordion: Status & Location ---
            if (ImGui.CollapsingHeader(this.loc.Translate("Section_Status"), ImGuiTreeNodeFlags.DefaultOpen)) {
                ImGui.Text($"{this.loc.Translate("Column_Status")}: ");
                ImGui.SameLine();
                ulong effectiveMask = friend.IsOnline ? friend.OnlineStateMask : 0;
                var statusInfo = this.gameDataService.GetOnlineStatusInfo(effectiveMask, friend.CurrentWorldId, friend.HomeWorldId, friend.LocationId);

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

                string displayLocation = this.loc.Translate("Profile_Unknown");
                if (friend.IsOnline) {
                    displayLocation = this.gameDataService.GetDisplayLocation(friend.LocationId, friend.CurrentWorldId, friend.HomeWorldId, friend.OnlineStateMask);
                    if (string.IsNullOrEmpty(displayLocation) || displayLocation == "0") {
                        displayLocation = this.loc.Translate("Profile_Unknown");
                    }
                }
                ImGui.Text($"{this.loc.Translate("Column_Location")}: {displayLocation}");

                string lastSeenStr = friend.IsOnline ? this.loc.Translate("Profile_Online") : (friend.LastSeenAt == DateTime.MinValue ? this.loc.Translate("Profile_Unknown") : this.loc.Translate("Profile_DaysAgo", (int)(DateTime.Now - friend.LastSeenAt).TotalDays));

                if (!friend.IsOnline && friend.LastSeenAt != DateTime.MinValue) {
                    var diff = DateTime.Now - friend.LastSeenAt;
                    if (diff.TotalDays < 1 && diff.TotalHours >= 1) {
                        lastSeenStr = this.loc.Translate("Profile_HoursAgo", (int)diff.TotalHours);
                    }
                    else if (diff.TotalHours < 1) {
                        lastSeenStr = this.loc.Translate("Profile_MinsAgo", (int)diff.TotalMinutes);
                    }
                }

                ImGui.Text($"{this.loc.Translate("Profile_LastSeen")}: {lastSeenStr}");
                ImGui.Spacing();
            }

            // --- Accordion: Character Information ---
            if (ImGui.CollapsingHeader(this.loc.Translate("Section_Character"), ImGuiTreeNodeFlags.DefaultOpen)) {
                ImGui.Text($"{this.loc.Translate("Profile_Job")}: ");
                ImGui.SameLine();

                if (friend.JobId > 0) {
                    var jobIconId = this.gameDataService.GetJobIconId(friend.JobId);
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
                var jobAbbr = friend.JobId > 0 ? this.gameDataService.GetJobAbbreviation(friend.JobId) : this.loc.Translate("Profile_None");
                ImGui.Text(jobAbbr);

                if (friend.Level > 0) {
                    ImGui.Text($"{this.loc.Translate("Profile_Level")}: {friend.Level}");
                }

                string title = this.gameDataService.GetTitleName(friend.TitleId, friend.Gender);
                if (!string.IsNullOrEmpty(title)) {
                    ImGui.Text($"{this.loc.Translate("Profile_Title")}: {title}");
                }

                string race = this.gameDataService.GetRaceName(friend.Race, friend.Gender);
                string tribe = this.gameDataService.GetTribeName(friend.Tribe, friend.Gender);
                if (!string.IsNullOrEmpty(race)) {
                    ImGui.Text($"{this.loc.Translate("Profile_Race")}: {race} ({tribe})");
                }

                if (friend.IsFantasiaDetected) {
                    ImGui.PushStyleColor(ImGuiCol.Text, this.themeService.CurrentPalette.TextMarkedForRemoval);
                    ImGui.Text(this.loc.Translate("Profile_FantasiaDetected"));
                    ImGui.PopStyleColor();
                    ImGui.SameLine();

                    if (ImGuiComponents.IconButton(FontAwesomeIcon.CheckDouble)) {
                        friend.IsFantasiaDetected = false;
                        this.friendRepository.Save();
                    }
                    if (ImGui.IsItemHovered()) {
                        ImGui.SetTooltip(this.loc.Translate("Action_ClearFantasia"));
                    }
                }

                string fcName = string.IsNullOrEmpty(friend.FcTag) ? this.loc.Translate("Profile_None") : friend.FcTag;
                ImGui.Text($"{this.loc.Translate("Profile_FC")}: {fcName}");

                ImGui.Text($"{this.loc.Translate("Profile_GrandCompany")}: ");
                ImGui.SameLine();
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

                string gcName = friend.GrandCompany > 0 ? this.gameDataService.GetGrandCompanyName(friend.GrandCompany) : this.loc.Translate("Profile_None");
                ImGui.Text(gcName);
                ImGui.Text($"{this.loc.Translate("Profile_HomeWorld")}: {this.gameDataService.GetWorldName(friend.HomeWorldId)}");
                ImGui.Text($"{this.loc.Translate("Profile_ClientLanguages")}: {this.gameDataService.GetClientLanguageString(friend.ClientLanguages)}");
                ImGui.Spacing();
            }

            // --- Accordion: System Data ---
            if (ImGui.CollapsingHeader(this.loc.Translate("Section_System"))) {
                var dateStr = friend.AddedAt == DateTime.MinValue ? this.loc.Translate("Profile_Unknown") : friend.AddedAt.ToShortDateString();
                var locStr = this.gameDataService.GetLocationName(friend.AddedLocationId);
                ImGui.Text($"{this.loc.Translate("Profile_Added")}: {dateStr}");
                ImGui.Text($"{this.loc.Translate("Profile_MetAt")}: {locStr}");

                string listStatus = friend.IsCharacterDeleted ? this.loc.Translate("Profile_StatusDeleted") : (friend.IsArchived ? this.loc.Translate("Profile_StatusArchived") : this.loc.Translate("Profile_StatusActive"));
                ImGui.Text($"{this.loc.Translate("Profile_ListStatus")}: {listStatus}");

                if (friend.PreviousNames != null && friend.PreviousNames.Count > 0) {
                    ImGui.Spacing();
                    ImGui.TextUnformatted(this.loc.Translate("Section_NameHistory"));
                    foreach (var oldName in friend.PreviousNames) {
                        ImGui.BulletText(oldName);
                    }
                }
                ImGui.Spacing();
            }

            // --- Accordion: Notes ---
            if (ImGui.CollapsingHeader(this.loc.Translate("Section_Notes"), ImGuiTreeNodeFlags.DefaultOpen)) {
                ImGui.InputTextMultiline("##notes", ref this.notesBuffer, 2048, new Vector2(-1, 100));
                if (ImGui.IsItemDeactivatedAfterEdit()) {
                    friend.Notes = this.notesBuffer;
                    this.friendRepository.Save();
                }
                ImGui.Spacing();
            }

        }
        ImGui.EndChild();
    }
}