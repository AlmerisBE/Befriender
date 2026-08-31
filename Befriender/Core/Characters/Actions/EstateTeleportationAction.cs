namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;

public unsafe class EstateTeleportationAction : ICharacterAction {
    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000015");
    public string InternalName => "Action_EstateTeleportation";
    public FontAwesomeIcon Icon => FontAwesomeIcon.HouseUser;

    public bool CanExecute(Character character) {
        // This native UI window generally requires the target to be in the actual friendlist
        return character.IsActivelyTracked && !string.IsNullOrEmpty(character.Name);
    }

    public void Execute(Character character) {
        var agent = AgentFriendlist.Instance();
        if (agent != null) {
            agent->OpenFriendEstateTeleportation(character.ContentId);
        }
    }
}