namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using System;

public class TrackCharacterAction : ICharacterAction {
    private ICharacterRegistry registry;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000020");
    public string InternalName => "Action_TrackFriend";
    public FontAwesomeIcon Icon => FontAwesomeIcon.Bell;

    public TrackCharacterAction(ICharacterRegistry registry) {
        this.registry = registry;
    }

    public bool CanExecute(Character character) {
        return character.IsActivelyTracked && !string.IsNullOrEmpty(character.Name) && !character.IsTrackedForNotifications;
    }

    public void Execute(Character character) {
        character.IsTrackedForNotifications = true;
        this.registry.SaveMasterList();
    }
}