namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;

public class UnmarkForRemovalAction : IFriendAction {
    private IFriendRepository friendRepository;

    public string InternalName => "Action_UnmarkForRemoval";
    public FontAwesomeIcon Icon => FontAwesomeIcon.Undo;

    public UnmarkForRemovalAction(IFriendRepository friendRepository) {
        this.friendRepository = friendRepository;
    }

    public bool CanExecute(FriendProfile friend) {
        return friend.IsMarkedForRemoval;
    }

    public void Execute(FriendProfile friend) {
        friend.IsMarkedForRemoval = false;
        this.friendRepository.Save();
    }
}