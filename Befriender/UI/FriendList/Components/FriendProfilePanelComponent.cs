namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

public class FriendProfilePanelComponent {
    private IGameDataService gameDataService;
    private IFriendRepository friendRepository;
    private string notesBuffer = string.Empty;
    private ulong currentFriendId = 0;

    public FriendProfilePanelComponent(IGameDataService gameDataService, IFriendRepository friendRepository) {
        this.gameDataService = gameDataService;
        this.friendRepository = friendRepository;
    }

    public void Draw(float panelWidth, float footerHeight, FriendProfile friend, Action onClose) {
        if (ImGui.BeginChild("ProfilePanel", new Vector2(panelWidth, -footerHeight), true)) {
            // Reset buffer if the selected friend changed
            if (this.currentFriendId != friend.ContentId) {
                this.currentFriendId = friend.ContentId;
                this.notesBuffer = friend.Notes ?? string.Empty;
            }

            ImGui.TextUnformatted(string.IsNullOrEmpty(friend.Name) ? "(Deleted Character)" : friend.Name);
            ImGui.SameLine(ImGui.GetContentRegionAvail().X - 20);
            if (ImGui.Button("X")) {
                onClose();
            }

            ImGui.Separator();
            ImGui.Spacing();

            var jobAbbr = friend.JobId > 0 ? this.gameDataService.GetJobAbbreviation(friend.JobId) : "None";
            ImGui.Text($"Job: {jobAbbr}");
            ImGui.Text($"World: {this.gameDataService.GetWorldName(friend.HomeWorldId)}");
            if (!string.IsNullOrEmpty(friend.FcTag)) {
                ImGui.Text($"Free Company: <{friend.FcTag}>");
            }

            ImGui.Text($"Languages: {this.gameDataService.GetClientLanguageString(friend.ClientLanguages)}");

            ImGui.Spacing();
            ImGui.Text("--- Metadata ---");
            var dateStr = friend.AddedAt == DateTime.MinValue ? "Unknown" : friend.AddedAt.ToShortDateString();
            var locStr = this.gameDataService.GetLocationName(friend.AddedLocationId);
            ImGui.Text($"Added: {dateStr}");
            ImGui.Text($"Met at: {locStr}");

            string lastSeenStr = friend.IsOnline ? "Online" : (friend.LastSeenAt == DateTime.MinValue ? "Unknown" : $"{(int)(DateTime.Now - friend.LastSeenAt).TotalDays} days ago");
            ImGui.Text($"Last Seen: {lastSeenStr}");

            ImGui.Spacing();
            string listStatus = friend.IsCharacterDeleted ? "Character Deleted" : (friend.IsArchived ? "Archived" : "Active");
            ImGui.Text($"List Status: {listStatus}");

            ImGui.Spacing();
            ImGui.Text("--- Notes ---");
            ImGui.InputTextMultiline("##notes", ref this.notesBuffer, 2048, new Vector2(-1, 100));
            if (ImGui.IsItemDeactivatedAfterEdit()) {
                friend.Notes = this.notesBuffer;
                this.friendRepository.Save();
            }

            if (friend.PreviousNames != null && friend.PreviousNames.Count > 0) {
                ImGui.Spacing();
                ImGui.Text("--- Name History ---");
                foreach (var oldName in friend.PreviousNames) {
                    ImGui.BulletText(oldName);
                }
            }

            ImGui.Spacing();
            ImGui.Separator();

            if (friend.IsArchived) {
                if (ImGui.Button("Restore Friend", new Vector2(-1, 0))) {
                    friend.IsArchived = false;
                    this.friendRepository.Save();
                }
            }
            else {
                if (ImGui.Button("Archive Friend", new Vector2(-1, 0))) {
                    friend.IsArchived = true;
                    this.friendRepository.Save();
                }
            }
        }
        ImGui.EndChild();
    }
}