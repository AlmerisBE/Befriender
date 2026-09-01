namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using System;

public class DeleteCharacterDataAction : ICharacterAction {
    private ICharacterRegistry registry;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000014");
    public string InternalName => "Action_DeleteData";
    public FontAwesomeIcon Icon => FontAwesomeIcon.TrashAlt;

    public DeleteCharacterDataAction(ICharacterRegistry registry) {
        this.registry = registry;
    }

    public bool CanExecute(Character character) {
        // We only allow permanent deletion if the character is no longer tracked by any active source
        return !character.IsActivelyTracked;
    }

    public void Execute(Character character) {
        this.registry.RemoveCharacter(character.Id);
        this.registry.SaveMasterList();
    }
}