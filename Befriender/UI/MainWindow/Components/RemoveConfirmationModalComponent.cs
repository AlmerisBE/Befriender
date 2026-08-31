namespace Befriender.UI.MainWindow.Components;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.UI.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

public class RemoveConfirmationModalComponent : IDisposable {
    private IRemoveCharacterRequestService requestService;
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private Character? pendingCharacter = null;
    private bool triggerOpen = false;

    public RemoveConfirmationModalComponent(IRemoveCharacterRequestService requestService, ICharacterRegistry registry, ILocalizationService loc) {
        this.requestService = requestService;
        this.registry = registry;
        this.loc = loc;
        this.requestService.OnRemoveRequested += this.HandleRemoveRequested;
    }

    private void HandleRemoveRequested(Character character) {
        this.pendingCharacter = character;
        this.triggerOpen = true;
    }

    public void Draw() {
        if (this.triggerOpen && this.pendingCharacter != null) {
            ImGui.OpenPopup("ConfirmRemovalPopup");
            this.triggerOpen = false;
        }

        Vector2 center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        bool isModalOpen = true;
        if (ImGui.BeginPopupModal("ConfirmRemovalPopup", ref isModalOpen, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove)) {
            if (this.pendingCharacter == null) {
                ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
                return;
            }

            string displayName = this.pendingCharacter.Name;
            if (string.IsNullOrEmpty(displayName)) {
                displayName = this.loc.Translate("Profile_DeletedCharacter");
                if (this.pendingCharacter.PreviousNames != null && this.pendingCharacter.PreviousNames.Count > 0) {
                    displayName += $" ({this.pendingCharacter.PreviousNames[0]})";
                }
            }

            ImGui.Text(this.loc.Translate("Modal_RemoveConfirmText", displayName));
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button(this.loc.Translate("Action_Confirm"), new Vector2(120, 0))) {
                this.pendingCharacter.IsMarkedForRemoval = true;
                this.registry.SaveMasterList();
                ImGui.CloseCurrentPopup();
                this.pendingCharacter = null;
            }

            ImGui.SetItemDefaultFocus();
            ImGui.SameLine();

            if (ImGui.Button(this.loc.Translate("Action_Cancel"), new Vector2(120, 0))) {
                ImGui.CloseCurrentPopup();
                this.pendingCharacter = null;
            }

            ImGui.EndPopup();
        }

        if (!isModalOpen) {
            this.pendingCharacter = null;
        }
    }

    public void Dispose() {
        this.requestService.OnRemoveRequested -= this.HandleRemoveRequested;
    }
}