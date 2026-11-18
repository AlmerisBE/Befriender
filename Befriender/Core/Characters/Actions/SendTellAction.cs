namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.GameData.Contracts;
using Dalamud.Interface;
using System;

public class SendTellAction : ICharacterAction {
    private IGameDataService gameDataService;
    private IChatBoxService chatBoxService;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000026");
    public string InternalName => "Action_SendTell";
    public FontAwesomeIcon Icon => FontAwesomeIcon.CommentDots;

    public SendTellAction(IGameDataService gameDataService, IChatBoxService chatBoxService) {
        this.gameDataService = gameDataService;
        this.chatBoxService = chatBoxService;
    }

    public bool CanExecute(Character character) {
        return !string.IsNullOrEmpty(character.Name);
    }

    public void Execute(Character character) {
        var worldName = this.gameDataService.GetWorldName(character.HomeWorldId);
        string command = $"/tell {character.Name}@{worldName} ";

        this.chatBoxService.SendMessage(command);
    }
}