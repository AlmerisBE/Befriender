namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
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

    private string notesBuffer = string.Empty;
    private ulong currentFriendId = 0;

    public FriendProfilePanelComponent(IGameDataService gameDataService, IFriendRepository friendRepository, ILocalizationService loc, IFriendActionService actionService, ITextureProvider textureProvider, IFriendGroupRepository groupRepository) {
        this.gameDataService = gameDataService;
        this.friendRepository = friendRepository;
        this.loc = loc;
        this.actionService = actionService;
        this.textureProvider = textureProvider;
        this.groupRepository = groupRepository;
    }

    public void Draw(float panelWidth, float footerHeight, FriendProfile friend, Action onClose) {
        if (ImGui.BeginChild("ProfilePanel", new Vector2(panelWidth, -footerHeight), true)) {
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

            // --- Custom Group Assignment ---
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

            ImGui.Spacing();

            // --- Status ---
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

            // --- Job / Class ---
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

            ImGui.Spacing();

            // --- Location ---
            string displayLocation = this.loc.Translate("Profile_Unknown");
            if (friend.IsOnline) {
                displayLocation = this.gameDataService.GetDisplayLocation(friend.LocationId, friend.CurrentWorldId, friend.HomeWorldId, friend.OnlineStateMask);
                if (string.IsNullOrEmpty(displayLocation) || displayLocation == "0") {
                    displayLocation = this.loc.Translate("Profile_Unknown");
                }
            }

            ImGui.Text($"{this.loc.Translate("Column_Location")}: {displayLocation}");

            // --- Free Company ---
            string fcName = string.IsNullOrEmpty(friend.FcTag) ? this.loc.Translate("Profile_None") : friend.FcTag;
            ImGui.Text($"{this.loc.Translate("Profile_FC")}: {fcName}");

            // --- Grand Company ---
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

            // --- Home World ---
            ImGui.Text($"{this.loc.Translate("Profile_HomeWorld")}: {this.gameDataService.GetWorldName(friend.HomeWorldId)}");

            // --- Client Languages ---
            ImGui.Text($"{this.loc.Translate("Profile_ClientLanguages")}: {this.gameDataService.GetClientLanguageString(friend.ClientLanguages)}");

            ImGui.Spacing();

            // --- Metadata ---
            ImGui.Text(this.loc.Translate("Profile_MetadataHeader"));

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
            var dateStr = friend.AddedAt == DateTime.MinValue ? this.loc.Translate("Profile_Unknown") : friend.AddedAt.ToShortDateString();
            var locStr = this.gameDataService.GetLocationName(friend.AddedLocationId);
            ImGui.Text($"{this.loc.Translate("Profile_Added")}: {dateStr}");
            ImGui.Text($"{this.loc.Translate("Profile_MetAt")}: {locStr}");

            ImGui.Spacing();
            string listStatus = friend.IsCharacterDeleted ? this.loc.Translate("Profile_StatusDeleted") : (friend.IsArchived ? this.loc.Translate("Profile_StatusArchived") : this.loc.Translate("Profile_StatusActive"));
            ImGui.Text($"{this.loc.Translate("Profile_ListStatus")}: {listStatus}");

            ImGui.Spacing();
            ImGui.Text(this.loc.Translate("Profile_NotesHeader"));
            ImGui.InputTextMultiline("##notes", ref this.notesBuffer, 2048, new Vector2(-1, 100));
            if (ImGui.IsItemDeactivatedAfterEdit()) {
                friend.Notes = this.notesBuffer;
                this.friendRepository.Save();
            }

            if (friend.PreviousNames != null && friend.PreviousNames.Count > 0) {
                ImGui.Spacing();
                ImGui.Text(this.loc.Translate("Profile_NameHistoryHeader"));
                foreach (var oldName in friend.PreviousNames) {
                    ImGui.BulletText(oldName);
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
        }
        ImGui.EndChild();
    }
}