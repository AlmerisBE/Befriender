namespace Befriender.UI.MainWindow.Components;

using Befriender.Core.Characters.Contracts;
using Befriender.UI.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using System.Linq;
using System.Numerics;

public class TagManagementComponent {
    private ICharacterTagRepository tagRepository;
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private string newTagBuffer = string.Empty;

    public TagManagementComponent(ICharacterTagRepository tagRepository, ICharacterRegistry registry, ILocalizationService loc) {
        this.tagRepository = tagRepository;
        this.registry = registry;
        this.loc = loc;
    }

    public void Draw() {
        ImGui.SetNextItemWidth(250);
        ImGui.InputTextWithHint("##newTag", this.loc.Translate("Tag_NewNameHint"), ref this.newTagBuffer, 30);
        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Plus) && !string.IsNullOrWhiteSpace(this.newTagBuffer)) {
            this.tagRepository.AddTag(this.newTagBuffer);
            this.newTagBuffer = string.Empty;
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        var tags = this.tagRepository.GetTags();
        var characters = this.registry.GetAllCharacters();

        if (tags.Count == 0) {
            ImGui.TextDisabled(this.loc.Translate("Tag_NoTags"));
            return;
        }

        if (ImGui.BeginTable("TagsManagementTable", 3, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY, new Vector2(0, -1))) {
            ImGui.TableSetupColumn(this.loc.Translate("Column_Name"));
            ImGui.TableSetupColumn(this.loc.Translate("Column_UsageCount"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Column_Actions"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            foreach (var tag in tags) {
                ImGui.TableNextRow();
                ImGui.PushID(tag.Id.ToString());

                ImGui.TableNextColumn();
                string nameBuffer = tag.Name;
                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputText("##tagName", ref nameBuffer, 30)) {
                    tag.Name = nameBuffer;
                    this.tagRepository.UpdateTag(tag);
                }

                ImGui.TableNextColumn();
                int usageCount = characters.Count(c => c.Tags.Contains(tag.Id));
                ImGui.Text(usageCount.ToString());

                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.TrashAlt)) {
                    var charsWithTag = characters.Where(c => c.Tags.Contains(tag.Id)).ToList();
                    foreach (var c in charsWithTag) {
                        c.Tags.Remove(tag.Id);
                    }

                    if (charsWithTag.Count > 0) {
                        this.registry.SaveMasterList();
                    }

                    this.tagRepository.RemoveTag(tag.Id);
                }

                if (ImGui.IsItemHovered()) {
                    ImGui.SetTooltip(this.loc.Translate("Tag_Delete"));
                }

                ImGui.PopID();
            }
            ImGui.EndTable();
        }
    }
}