namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;

public class TrackFriendAction : IFriendAction {
    private IFriendRepository friendRepository;

    public string InternalName => "Action_TrackFriend";
    public FontAwesomeIcon Icon => FontAwesomeIcon.Bell;

    public TrackFriendAction(IFriendRepository friendRepository) {
        this.friendRepository = friendRepository;
    }

    public bool CanExecute(FriendProfile friend) {
        return !friend.IsArchived && !friend.IsCharacterDeleted && !friend.IsTrackedForNotifications;
    }

    public void Execute(FriendProfile friend) {
        friend.IsTrackedForNotifications = true;
        this.friendRepository.Save();
    }
}