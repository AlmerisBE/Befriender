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
        // We allow native removal as long as the friend occupies a vanilla slot (not archived),
        // regardless of whether the character has been deleted by its owner.
        return !friend.IsArchived;
    }

    public void Execute(FriendProfile friend) {
        this.requestService.RequestRemoval(friend);
    }
}