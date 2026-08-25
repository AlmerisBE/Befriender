namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

public unsafe class SendTellAction : IFriendAction {
    private IGameDataService gameDataService;

    public string InternalName => "Action_SendTell";
    public FontAwesomeIcon Icon => FontAwesomeIcon.CommentDots;

    public SendTellAction(IGameDataService gameDataService) {
        this.gameDataService = gameDataService;
    }

    public bool CanExecute(FriendProfile friend) {
        return friend.IsOnline && !friend.IsCharacterDeleted;
    }

    public void Execute(FriendProfile friend) {
        var worldName = this.gameDataService.GetWorldName(friend.CurrentWorldId > 0 ? friend.CurrentWorldId : friend.HomeWorldId);
        string command = $"/tell {friend.Name}@{worldName} ";

        using var cmd = new Utf8String(command);

        cmd.SanitizeString(
            AllowedEntities.Unknown9 |
            AllowedEntities.Payloads |
            AllowedEntities.OtherCharacters |
            AllowedEntities.SpecialCharacters |
            AllowedEntities.Numbers |
            AllowedEntities.LowercaseLetters |
            AllowedEntities.UppercaseLetters);

        var uiModule = UIModule.Instance();
        if (uiModule != null) {
            uiModule->ProcessChatBoxEntry(&cmd);
        }
    }
}