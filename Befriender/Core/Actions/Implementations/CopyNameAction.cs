namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;
using Dalamud.Plugin.Services;

public class CopyNameAction : IFriendAction {
    private IChatGui chatGui;

    public string InternalName => "Action_CopyName";
    public FontAwesomeIcon Icon => FontAwesomeIcon.Copy;

    public CopyNameAction(IChatGui chatGui) {
        this.chatGui = chatGui;
    }

    public bool CanExecute(FriendProfile friend) {
        return !string.IsNullOrEmpty(friend.Name) && !friend.IsCharacterDeleted;
    }

    public void Execute(FriendProfile friend) {
        Dalamud.Bindings.ImGui.ImGui.SetClipboardText(friend.Name);
        this.chatGui.Print($"[Befriender] Copied {friend.Name} to clipboard.");
    }
}