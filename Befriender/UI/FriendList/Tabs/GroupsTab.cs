namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using System.Linq;
using System.Numerics;

public class GroupsTab : ITab {
    private IFriendGroupRepository groupRepository;
    private IFriendRepository friendRepository;
    private ILocalizationService loc;

    private string newGroupBuffer = string.Empty;

    public string InternalName => "Tab_Groups";
    public string Name => this.loc.Translate("Tab_Groups");
    public bool IsProfilePanelOpen => false;

    public GroupsTab(IFriendGroupRepository groupRepository, IFriendRepository friendRepository, ILocalizationService loc) {
        this.groupRepository = groupRepository;
        this.friendRepository = friendRepository;
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

        var groups = this.groupRepository.GetGroups();
        var friends = this.friendRepository.GetFriends();

        if (groups.Count == 0) {
            ImGui.TextDisabled(this.loc.Translate("Group_NoGroups"));
            return;
        }

        if (ImGui.BeginTabBar("GroupsSubTabBar")) {
            foreach (var group in groups) {
                if (ImGui.BeginTabItem($"{group.Title}###Group_{group.Id}")) {
                    ImGui.Spacing();

                    string titleBuffer = group.Title;
                    ImGui.SetNextItemWidth(300);
                    if (ImGui.InputText(this.loc.Translate("Group_Title"), ref titleBuffer, 50)) {
                        group.Title = titleBuffer;
                        this.groupRepository.UpdateGroup(group);
                    }

                    string descBuffer = group.Description;
                    ImGui.SetNextItemWidth(300);
                    if (ImGui.InputTextMultiline(this.loc.Translate("Group_Description"), ref descBuffer, 255, new Vector2(300, 60))) {
                        group.Description = descBuffer;
                        this.groupRepository.UpdateGroup(group);
                    }

                    ImGui.Spacing();
                    if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.TrashAlt, this.loc.Translate("Group_Delete"))) {
                        // Unassign all friends in this group before deleting
                        var friendsInGroup = friends.Where(f => f.CustomGroupId == group.Id).ToList();
                        foreach (var f in friendsInGroup) {
                            f.CustomGroupId = null;
                        }

                        if (friendsInGroup.Count > 0) {
                            this.friendRepository.Save();
                        }

                        this.groupRepository.RemoveGroup(group.Id);
                    }

                    ImGui.Spacing();
                    ImGui.Separator();
                    ImGui.Spacing();

                    var groupFriends = friends.Where(f => f.CustomGroupId == group.Id && !f.IsArchived).ToList();
                    ImGui.Text($"{this.loc.Translate("Group_Members")} ({groupFriends.Count})");

                    if (ImGui.BeginListBox($"##GroupList_{group.Id}", new Vector2(-1, -1))) {
                        foreach (var friend in groupFriends) {
                            ImGui.Text(friend.Name);
                        }

                        ImGui.EndListBox();
                    }

                    ImGui.EndTabItem();
                }
            }
            ImGui.EndTabBar();
        }
    }
}