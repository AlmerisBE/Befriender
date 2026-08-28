namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using System.Linq;
using System.Numerics;

public class TagManagementComponent {
    private IFriendTagRepository tagRepository;
    private IFriendRepository friendRepository;
    private ILocalizationService loc;

    private string newTagBuffer = string.Empty;

    public TagManagementComponent(IFriendTagRepository tagRepository, IFriendRepository friendRepository, ILocalizationService loc) {
        this.tagRepository = tagRepository;
        this.friendRepository = friendRepository;
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
        var friends = this.friendRepository.GetFriends();

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

                // --- Name Column ---
                ImGui.TableNextColumn();
                string nameBuffer = tag.Name;
                ImGui.SetNextItemWidth(-1);
                if (ImGui.InputText("##tagName", ref nameBuffer, 30)) {
                    tag.Name = nameBuffer;
                    this.tagRepository.UpdateTag(tag);
                }

                // --- Usage Column ---
                ImGui.TableNextColumn();
                int usageCount = friends.Count(f => f.Tags.Contains(tag.Id));
                ImGui.Text(usageCount.ToString());

                // --- Actions Column ---
                ImGui.TableNextColumn();
                if (ImGuiComponents.IconButton(FontAwesomeIcon.TrashAlt)) {
                    // Remove tag from all friends before deleting
                    var friendsWithTag = friends.Where(f => f.Tags.Contains(tag.Id)).ToList();
                    foreach (var f in friendsWithTag) {
                        f.Tags.Remove(tag.Id);
                    }

                    if (friendsWithTag.Count > 0) {
                        this.friendRepository.Save();
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