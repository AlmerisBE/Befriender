namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;

public class RequestRemoveFriendAction : IFriendAction {
    private IRemoveFriendRequestService requestService;

    public string InternalName => "Action_RemoveFriend";
    public FontAwesomeIcon Icon => FontAwesomeIcon.UserTimes;

    public RequestRemoveFriendAction(IRemoveFriendRequestService requestService) {
        this.requestService = requestService;
    }

    public bool CanExecute(FriendProfile friend) {
        return !friend.IsArchived && !friend.IsMarkedForRemoval;
    }

    public void Execute(FriendProfile friend) {
        this.requestService.RequestRemoval(friend);
    }
}