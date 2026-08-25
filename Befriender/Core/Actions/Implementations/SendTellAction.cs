namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Dalamud.Interface;
using Dalamud.Plugin.Services;

public class SendTellAction : IFriendAction {
    private IGameDataService gameDataService;
    private IChatGui chatGui;

    public string InternalName => "Action_SendTell";
    public FontAwesomeIcon Icon => FontAwesomeIcon.CommentDots;

    public SendTellAction(IGameDataService gameDataService, IChatGui chatGui) {
        this.gameDataService = gameDataService;
        this.chatGui = chatGui;
    }

    public bool CanExecute(FriendProfile friend) {
        return friend.IsOnline && !friend.IsCharacterDeleted;
    }

    public void Execute(FriendProfile friend) {
        var worldName = this.gameDataService.GetWorldName(friend.CurrentWorldId > 0 ? friend.CurrentWorldId : friend.HomeWorldId);
        string command = $"/tell {friend.Name}@{worldName} ";

        Dalamud.Bindings.ImGui.ImGui.SetClipboardText(command);
        this.chatGui.Print($"[Befriender] Ready to chat. Command copied to clipboard: {command}");
    }
}