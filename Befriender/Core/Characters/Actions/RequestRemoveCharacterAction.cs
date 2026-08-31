namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using System;

public class RequestRemoveCharacterAction : ICharacterAction {
    private IRemoveCharacterRequestService requestService;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000012");
    public string InternalName => "Action_RemoveFriend";
    public FontAwesomeIcon Icon => FontAwesomeIcon.UserTimes;

    public RequestRemoveCharacterAction(IRemoveCharacterRequestService requestService) {
        this.requestService = requestService;
    }

    public bool CanExecute(Character character) {
        return character.IsActivelyTracked && !character.IsMarkedForRemoval;
    }

    public void Execute(Character character) {
        this.requestService.RequestRemoval(character);
    }
}