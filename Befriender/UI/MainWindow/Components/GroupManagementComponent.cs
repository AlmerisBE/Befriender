namespace Befriender.UI.MainWindow.Components;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using System;
using System.Linq;
using System.Numerics;

public class GroupManagementComponent {
    private ICharacterGroupRepository groupRepository;
    private ICharacterRegistry registry;
    private ILocalizationService loc;

    private string newGroupBuffer = string.Empty;
    private Guid? groupToOpen = null;

    public string InternalName => "Tab_Groups";
    public string Name => this.loc.Translate("Tab_Groups");
    public bool IsProfilePanelOpen => false;

    public GroupManagementComponent(ICharacterGroupRepository groupRepository, ICharacterRegistry registry, ILocalizationService loc) {
        this.groupRepository = groupRepository;
        this.registry = registry;
        this.loc = loc;
    }

    public void Draw() {
        ImGui.SetNextItemWidth(250);
        ImGui.InputTextWithHint("##newGroup", this.loc.Translate("Group_NewNameHint"), ref this.newGroupBuffer, 50);
        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus) && !string.IsNullOrWhiteSpace(this.newGroupBuffer)) {
            this.groupRepository.AddGroup(this.newGroupBuffer);
            this.newGroupBuffer = string.Empty;
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var groups = this.groupRepository.GetGroups().ToList();
        var characters = this.registry.GetAllCharacters();

        if (groups.Count == 0) {
            ImGui.TextDisabled(this.loc.Translate("Group_NoGroups"));
            return;
        }

        for (int i = 0; i < groups.Count; i++) {
            var group = groups[i];
            ImGui.PushID(group.Id.ToString());

            if (this.groupToOpen == group.Id) {
                ImGui.SetNextItemOpen(true);
                this.groupToOpen = null;
            }

            if (ImGui.CollapsingHeader($"{group.Title}###Header_{group.Id}")) {
                ImGui.Spacing();

                bool canMoveUp = i > 0;
                bool canMoveDown = i < groups.Count - 1;
                float frameHeight = ImGui.GetFrameHeight();

                if (!canMoveUp) {
                    ImGui.BeginDisabled();
                }

                if (ImGuiComponents.IconButton(FontAwesomeIcon.ArrowUp)) {
                    this.groupRepository.MoveGroupUp(group.Id);
                    this.groupToOpen = group.Id;
                }
                if (!canMoveUp) {
                    ImGui.EndDisabled();
                }

                ImGui.SameLine();
                if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.TrashAlt, this.loc.Translate("Group_Delete"))) {
                    var charsInGroup = characters.Where(c => c.CustomGroupId == group.Id).ToList();
                    foreach (var c in charsInGroup) {
                        c.CustomGroupId = null;
                    }

                    if (charsInGroup.Count > 0) {
                        this.registry.SaveMasterList();
                    }

                    this.groupRepository.RemoveGroup(group.Id);
                }

                ImGui.SameLine(ImGui.GetWindowContentRegionMax().X - frameHeight);
                if (!canMoveDown) {
                    ImGui.BeginDisabled();
                }

                if (ImGuiComponents.IconButton(FontAwesomeIcon.ArrowDown)) {
                    this.groupRepository.MoveGroupDown(group.Id);
                    this.groupToOpen = group.Id;
                }
                if (!canMoveDown) {
                    ImGui.EndDisabled();
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                string titleBuffer = group.Title;
                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputText(this.loc.Translate("Group_Title"), ref titleBuffer, 50)) {
                    group.Title = titleBuffer;
                    this.groupRepository.UpdateGroup(group);
                }

                string descBuffer = group.Description;
                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputTextMultiline(this.loc.Translate("Group_Description"), ref descBuffer, 255, new Vector2(-1, 80))) {
                    group.Description = descBuffer;
                    this.groupRepository.UpdateGroup(group);
                }

                ImGui.Spacing();
                ImGui.Separator();
                ImGui.Spacing();

                var groupChars = characters.Where(c => c.CustomGroupId == group.Id && c.IsActivelyTracked).ToList();
                ImGui.Text($"{this.loc.Translate("Group_Members")} ({groupChars.Count})");

                if (ImGui.BeginListBox($"##GroupList_{group.Id}", new Vector2(-1, -1))) {
                    foreach (var c in groupChars) {
                        ImGui.Text(c.Name);
                    }

                    ImGui.EndListBox();
                }
                ImGui.Spacing();
            }
            ImGui.PopID();
        }
    }
}