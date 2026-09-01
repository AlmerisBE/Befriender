namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using System;

public class UntrackCharacterAction : ICharacterAction {
    private ICharacterRegistry registry;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000021");
    public string InternalName => "Action_UntrackFriend";
    public FontAwesomeIcon Icon => FontAwesomeIcon.BellSlash;

    public UntrackCharacterAction(ICharacterRegistry registry) {
        this.registry = registry;
    }

    public bool CanExecute(Character character) {
        return character.IsActivelyTracked && !string.IsNullOrEmpty(character.Name) && character.IsTrackedForNotifications;
    }

    public void Execute(Character character) {
        character.IsTrackedForNotifications = false;
        this.registry.SaveMasterList();
    }
}