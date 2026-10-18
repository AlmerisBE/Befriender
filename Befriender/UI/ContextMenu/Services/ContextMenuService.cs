namespace Befriender.UI.ContextMenu.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Linq;

public class ContextMenuService : IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IContextMenu contextMenu;
    private ICharacterRegistry registry;
    private ICharacterGroupRepository groupRepository;
    private ICharacterTagRepository tagRepository;
    private ILocalizationService loc;
    private IClientState clientState;
    private IObjectTable objectTable;

    private Character? quickAssignTarget = null;
    private bool openQuickAssignPopup = false;

    public ContextMenuService(
        IDalamudPluginInterface pluginInterface,
        IContextMenu contextMenu,
        ICharacterRegistry registry,
        ICharacterGroupRepository groupRepository,
        ICharacterTagRepository tagRepository,
        ILocalizationService loc,
        IClientState clientState,
        IObjectTable objectTable) {

        this.pluginInterface = pluginInterface;
        this.contextMenu = contextMenu;
        this.registry = registry;
        this.groupRepository = groupRepository;
        this.tagRepository = tagRepository;
        this.loc = loc;
        this.clientState = clientState;
        this.objectTable = objectTable;

        this.contextMenu.OnMenuOpened += this.OnMenuOpened;
        this.pluginInterface.UiBuilder.Draw += this.DrawPopup;
    }

    private void OnMenuOpened(IMenuOpenedArgs args) {
        if (args.Target is not MenuTargetDefault target) return;

        string name = target.TargetName;
        uint homeWorldId = target.TargetHomeWorld.RowId;

        if (string.IsNullOrWhiteSpace(name) || homeWorldId == 0) return;

        // Perform explicit matching to ensure we find the character, avoiding any edge cases with ContentId=0
        var existing = this.registry.GetAllCharacters().FirstOrDefault(c =>
            string.Equals(c.Name, name, StringComparison.Ordinal) && c.HomeWorldId == homeWorldId);

        if (existing != null) {
            args.AddMenuItem(new MenuItem {
                Name = this.loc.Translate("Action_QuickAssign"),
                OnClicked = (a) => this.OnQuickAssignClicked(existing)
            });
        }

        if (existing == null || !existing.IsActivelyTracked) {
            args.AddMenuItem(new MenuItem {
                Name = this.loc.Translate("Action_AddToBefriender"),
                OnClicked = (a) => this.OnAddClicked(target)
            });
        }
    }

    private void OnQuickAssignClicked(Character character) {
        this.quickAssignTarget = character;
        this.openQuickAssignPopup = true;
    }

    private void OnAddClicked(MenuTargetDefault target) {
        string name = target.TargetName;
        uint homeWorldId = target.TargetHomeWorld.RowId;
        ulong contentId = 0;

        if (target.TargetObjectId > 0) {
            var obj = this.objectTable.SearchById(target.TargetObjectId);
            if (obj is IPlayerCharacter pc) {
                unsafe {
                    var csChar = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)pc.Address;
                    if (csChar != null) contentId = csChar->AccountId;
                }
            }
        }

        uint locationId = this.clientState.TerritoryType;
        this.registry.TryAddManualCharacter(contentId, name, homeWorldId, locationId);
    }

    private void DrawPopup() {
        if (this.openQuickAssignPopup && this.quickAssignTarget != null) {
            ImGui.OpenPopup("BefrienderQuickAssignPopup");
            this.openQuickAssignPopup = false;
        }

        // Force the ImGui popup to spawn directly at the cursor to maintain UX flow
        ImGui.SetNextWindowPos(ImGui.GetMousePos(), ImGuiCond.Appearing);

        if (ImGui.BeginPopup("BefrienderQuickAssignPopup")) {
            if (this.quickAssignTarget == null) {
                ImGui.CloseCurrentPopup();
                ImGui.EndPopup();
                return;
            }

            ImGui.TextDisabled(this.quickAssignTarget.Name);
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextUnformatted(this.loc.Translate("Tab_Groups"));
            var groups = this.groupRepository.GetGroups();

            bool noGroup = this.quickAssignTarget.CustomGroupId == null;
            if (ImGui.RadioButton(this.loc.Translate("Group_None"), noGroup)) {
                this.quickAssignTarget.CustomGroupId = null;
                this.registry.SaveMasterList();
            }

            foreach (var group in groups) {
                bool isSelected = this.quickAssignTarget.CustomGroupId == group.Id;
                if (ImGui.RadioButton($"{group.Title}##group_{group.Id}", isSelected)) {
                    this.quickAssignTarget.CustomGroupId = group.Id;
                    this.registry.SaveMasterList();
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextUnformatted(this.loc.Translate("Tab_Tags"));
            var tags = this.tagRepository.GetTags();

            if (tags.Count == 0) ImGui.TextDisabled(this.loc.Translate("Tag_NoTagsCreated"));
            else {
                foreach (var tag in tags) {
                    bool hasTag = this.quickAssignTarget.Tags.Contains(tag.Id);
                    if (ImGui.Checkbox($"{tag.Name}##tag_{tag.Id}", ref hasTag)) {
                        if (hasTag) this.quickAssignTarget.Tags.Add(tag.Id);
                        else this.quickAssignTarget.Tags.Remove(tag.Id);
                        this.registry.SaveMasterList();
                    }
                }
            }

            ImGui.EndPopup();
        }
        else this.quickAssignTarget = null;
    }

    public void Dispose() {
        this.contextMenu.OnMenuOpened -= this.OnMenuOpened;
        this.pluginInterface.UiBuilder.Draw -= this.DrawPopup;
    }
}