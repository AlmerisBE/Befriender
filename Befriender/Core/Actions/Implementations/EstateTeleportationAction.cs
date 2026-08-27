namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;

public unsafe class EstateTeleportationAction : IFriendAction {
    public string InternalName => "Action_EstateTeleportation";
    public FontAwesomeIcon Icon => FontAwesomeIcon.HouseUser;

    public bool CanExecute(FriendProfile friend) {
        return !friend.IsArchived && !friend.IsCharacterDeleted;
    }

    public void Execute(FriendProfile friend) {
        var agent = AgentFriendlist.Instance();
        if (agent != null) {
            agent->OpenFriendEstateTeleportation(friend.ContentId);
        }
    }
}