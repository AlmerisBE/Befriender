namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;

public class UntrackFriendAction : IFriendAction {
    private IFriendRepository friendRepository;

    public string InternalName => "Action_UntrackFriend";
    public FontAwesomeIcon Icon => FontAwesomeIcon.BellSlash;

    public UntrackFriendAction(IFriendRepository friendRepository) {
        this.friendRepository = friendRepository;
    }

    public bool CanExecute(FriendProfile friend) {
        return !friend.IsCharacterDeleted && friend.IsTrackedForNotifications;
    }

    public void Execute(FriendProfile friend) {
        friend.IsTrackedForNotifications = false;
        this.friendRepository.Save();
    }
}