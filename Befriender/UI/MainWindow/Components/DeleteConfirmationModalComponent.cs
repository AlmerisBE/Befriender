namespace Befriender.UI.MainWindow.Components;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using System;
using System.Numerics;

public class DeleteConfirmationModalComponent : IDisposable {
    private IDeleteCharacterDataRequestService requestService;
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private Character? pendingCharacter = null;
    private bool triggerOpen = false;

    public DeleteConfirmationModalComponent(IDeleteCharacterDataRequestService requestService, ICharacterRegistry registry, ILocalizationService loc) {
        this.requestService = requestService;
        this.registry = registry;
        this.loc = loc;
        this.requestService.OnDeleteRequested += this.HandleDeleteRequested;
    }

    private void HandleDeleteRequested(Character character) {
        this.pendingCharacter = character;
        this.triggerOpen = true;
    }

    public void Draw() {
        if (this.triggerOpen && this.pendingCharacter != null) {
            ImGui.OpenPopup("ConfirmPermanentDeletionPopup");
            this.triggerOpen = false;
        }

        Vector2 center = ImGui.GetMainViewport().GetCenter();
        ImGui.SetNextWindowPos(center, ImGuiCond.Appearing, new Vector2(0.5f, 0.5f));

        bool isModalOpen = true;
        if (ImGui.BeginPopupModal("ConfirmPermanentDeletionPopup", ref isModalOpen, ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoMove)) {
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

            ImGui.Text(this.loc.Translate("Modal_DeleteConfirmText", displayName));
            ImGui.Separator();
            ImGui.Spacing();

            if (ImGui.Button(this.loc.Translate("Action_Confirm"), new Vector2(120, 0))) {
                // The registry now handles internal state logic and disk saving automatically.
                this.registry.RemoveCharacter(this.pendingCharacter.Id);
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

        if (!isModalOpen) this.pendingCharacter = null;
    }

    public void Dispose() {
        this.requestService.OnDeleteRequested -= this.HandleDeleteRequested;
    }
}