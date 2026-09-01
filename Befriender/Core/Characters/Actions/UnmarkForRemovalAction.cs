namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using System;

public class UnmarkForRemovalAction : ICharacterAction {
    private ICharacterRegistry registry;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000022");
    public string InternalName => "Action_UnmarkForRemoval";
    public FontAwesomeIcon Icon => FontAwesomeIcon.Undo;

    public UnmarkForRemovalAction(ICharacterRegistry registry) {
        this.registry = registry;
    }

    public bool CanExecute(Character character) {
        return character.IsMarkedForRemoval;
    }

    public void Execute(Character character) {
        character.IsMarkedForRemoval = false;
        this.registry.SaveMasterList();
    }
}