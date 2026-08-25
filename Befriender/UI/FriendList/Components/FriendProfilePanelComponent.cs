namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

public class FriendProfilePanelComponent {
    private IGameDataService gameDataService;
    private IFriendRepository friendRepository;
    private ILocalizationService loc;
    private string notesBuffer = string.Empty;
    private ulong currentFriendId = 0;

    public FriendProfilePanelComponent(IGameDataService gameDataService, IFriendRepository friendRepository, ILocalizationService loc) {
        this.gameDataService = gameDataService;
        this.friendRepository = friendRepository;
        this.loc = loc;
    }

    public void Draw(float panelWidth, float footerHeight, FriendProfile friend, Action onClose) {
        if (ImGui.BeginChild("ProfilePanel", new Vector2(panelWidth, -footerHeight), true)) {
            if (this.currentFriendId != friend.ContentId) {
                this.currentFriendId = friend.ContentId;
                this.notesBuffer = friend.Notes ?? string.Empty;
            }

            ImGui.TextUnformatted(string.IsNullOrEmpty(friend.Name) ? this.loc.Translate("Profile_DeletedCharacter") : friend.Name);
            ImGui.SameLine(ImGui.GetContentRegionAvail().X - 20);
            if (ImGui.Button("X")) {
                onClose();
            }

            ImGui.Separator();
            ImGui.Spacing();

            var jobAbbr = friend.JobId > 0 ? this.gameDataService.GetJobAbbreviation(friend.JobId) : "None";
            ImGui.Text($"{this.loc.Translate("Profile_Job")}: {jobAbbr}");
            ImGui.Text($"{this.loc.Translate("Profile_World")}: {this.gameDataService.GetWorldName(friend.HomeWorldId)}");
            if (!string.IsNullOrEmpty(friend.FcTag)) {
                ImGui.Text($"{this.loc.Translate("Profile_FC")}: <{friend.FcTag}>");
            }

            ImGui.Text($"{this.loc.Translate("Profile_Languages")}: {this.gameDataService.GetClientLanguageString(friend.ClientLanguages)}");

            ImGui.Spacing();
            ImGui.Text(this.loc.Translate("Profile_MetadataHeader"));
            var dateStr = friend.AddedAt == DateTime.MinValue ? this.loc.Translate("Profile_Unknown") : friend.AddedAt.ToShortDateString();
            var locStr = this.gameDataService.GetLocationName(friend.AddedLocationId);
            ImGui.Text($"{this.loc.Translate("Profile_Added")}: {dateStr}");
            ImGui.Text($"{this.loc.Translate("Profile_MetAt")}: {locStr}");

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

            if (friend.IsArchived) {
                if (ImGui.Button(this.loc.Translate("Profile_RestoreBtn"), new Vector2(-1, 0))) {
                    friend.IsArchived = false;
                    this.friendRepository.Save();
                }
            }
            else {
                if (ImGui.Button(this.loc.Translate("Profile_ArchiveBtn"), new Vector2(-1, 0))) {
                    friend.IsArchived = true;
                    this.friendRepository.Save();
                }
            }
        }
        ImGui.EndChild();
    }
}