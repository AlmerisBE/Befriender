namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

public class RemoveConfirmationModalComponent : IDisposable {
    private IRemoveFriendRequestService requestService;
    private IFriendRepository friendRepository;
    private ILocalizationService loc;
    private FriendProfile? pendingFriend = null;
    private bool triggerOpen = false;

    public RemoveConfirmationModalComponent(IRemoveFriendRequestService requestService, IFriendRepository friendRepository, ILocalizationService loc) {
        this.requestService = requestService;
        this.friendRepository = friendRepository;
        this.loc = loc;
        this.requestService.OnRemoveRequested += this.HandleRemoveRequested;
    }

    private void HandleRemoveRequested(FriendProfile friend) {
        this.pendingFriend = friend;
        this.triggerOpen = true;
    }

    public void Draw() {
        if (this.triggerOpen && this.pendingFriend != null) {
            ImGui.OpenPopup("ConfirmRemovalPopup");
            this.triggerOpen = false;
        }

        Vector2 center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        bool isModalOpen = true;
        if (ImGui.BeginPopupModal("ConfirmRemovalPopup", ref isModalOpen, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove)) {
            if (this.pendingFriend == null) {
                ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
                return;
            }

            string displayName = this.pendingFriend.Name;
            if (this.pendingFriend.IsCharacterDeleted || string.IsNullOrEmpty(displayName)) {
                displayName = this.loc.Translate("Profile_DeletedCharacter");
                if (this.pendingFriend.PreviousNames != null && this.pendingFriend.PreviousNames.Count > 0) {
                    displayName += $" ({this.pendingFriend.PreviousNames[0]})";
                }
            }

            ImGui.Text(this.loc.Translate("Modal_RemoveConfirmText", displayName));
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button(this.loc.Translate("Action_Confirm"), new Vector2(120, 0))) {
                this.pendingFriend.IsMarkedForRemoval = true;
                this.friendRepository.Save();
                ImGui.CloseCurrentPopup();
                this.pendingFriend = null;
            }

            ImGui.SetItemDefaultFocus();
            ImGui.SameLine();

            if (ImGui.Button(this.loc.Translate("Action_Cancel"), new Vector2(120, 0))) {
                ImGui.CloseCurrentPopup();
                this.pendingFriend = null;
            }

            ImGui.EndPopup();
        }

        if (!isModalOpen) {
            this.pendingFriend = null;
        }
    }

    public void Dispose() {
        this.requestService.OnRemoveRequested -= this.HandleRemoveRequested;
    }
}