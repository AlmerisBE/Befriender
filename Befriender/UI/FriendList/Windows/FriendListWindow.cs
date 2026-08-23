namespace Befriender.UI.FriendList.Windows;

using Befriender.Core.Friends.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System.Numerics;

public class FriendListWindow : Window {
    private IFriendRepository friendRepository;

    public FriendListWindow(IFriendRepository friendRepository) : base("Befriender - Friend List", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {
        this.friendRepository = friendRepository;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(500, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        var friends = this.friendRepository.GetFriends();

        if (friends.Count == 0) {
            ImGui.Text("Your friend list is currently empty or syncing...");
            return;
        }

        if (ImGui.BeginTable("FriendsTable", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY)) {
            ImGui.TableSetupColumn("Status");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Job");
            ImGui.TableSetupColumn("FC");
            ImGui.TableSetupColumn("World");
            ImGui.TableHeadersRow();

            foreach (var friend in friends) {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text(friend.IsOnline ? "Online" : "Offline");

                ImGui.TableNextColumn();
                ImGui.Text(friend.Name);

                ImGui.TableNextColumn();
                ImGui.Text(friend.JobId.ToString());

                ImGui.TableNextColumn();
                if (!string.IsNullOrEmpty(friend.FcTag)) {
                    ImGui.Text($"<{friend.FcTag}>");
                }
                else {
                    ImGui.Text(string.Empty);
                }

                ImGui.TableNextColumn();
                ImGui.Text(friend.HomeWorldId.ToString());
            }

            ImGui.EndTable();
        }
    }
}