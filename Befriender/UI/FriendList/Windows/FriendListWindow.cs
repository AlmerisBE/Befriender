namespace Befriender.UI.FriendList.Windows;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System.Numerics;

public class FriendListWindow : Window {
    public FriendListWindow() : base("Befriender - Friend List", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse) {
        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(400, 600),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        ImGui.Text("The friend list will instantly appear here.");
    }
}