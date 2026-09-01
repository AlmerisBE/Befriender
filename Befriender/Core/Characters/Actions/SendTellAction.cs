namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.GameData.Contracts;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;

public unsafe class SendTellAction : ICharacterAction {
    private IGameDataService gameDataService;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000026");
    public string InternalName => "Action_SendTell";
    public FontAwesomeIcon Icon => FontAwesomeIcon.CommentDots;

    public SendTellAction(IGameDataService gameDataService) {
        this.gameDataService = gameDataService;
    }

    public bool CanExecute(Character character) {
        return !string.IsNullOrEmpty(character.Name);
    }

    public void Execute(Character character) {
        var worldName = this.gameDataService.GetWorldName(character.CurrentWorldId > 0 ? character.CurrentWorldId : character.HomeWorldId);
        string command = $"/tell {character.Name}@{worldName} ";

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