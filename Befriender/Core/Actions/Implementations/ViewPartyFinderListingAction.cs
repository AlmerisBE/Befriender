namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

public unsafe class ViewPartyFinderListingAction : IFriendAction {
    public string InternalName => "Action_ViewPartyFinder";
    public FontAwesomeIcon Icon => FontAwesomeIcon.UsersViewfinder;

    public bool CanExecute(FriendProfile friend) {
        if (friend.IsCharacterDeleted || !friend.IsOnline) {
            return false;
        }

        var state = (InfoProxyCommonList.CharacterData.OnlineStatus)friend.OnlineStateMask;
        return state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.RecruitingPartyMembers);
    }

    public void Execute(FriendProfile friend) {
        var agent = AgentLookingForGroup.Instance();
        if (agent != null) {
            agent->OpenListingByContentId(friend.ContentId);
        }
    }
}