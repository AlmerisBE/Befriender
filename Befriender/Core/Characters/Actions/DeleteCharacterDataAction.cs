namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using System;

public class DeleteCharacterDataAction : ICharacterAction {
    private IDeleteCharacterDataRequestService requestService;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000014");
    public string InternalName => "Action_DeleteData";
    public FontAwesomeIcon Icon => FontAwesomeIcon.TrashAlt;

    public DeleteCharacterDataAction(IDeleteCharacterDataRequestService requestService) {
        this.requestService = requestService;
    }

    public bool CanExecute(Character character) {
        return !character.IsActivelyTracked;
    }

    public void Execute(Character character) {
        this.requestService.RequestDeletion(character);
    }
}