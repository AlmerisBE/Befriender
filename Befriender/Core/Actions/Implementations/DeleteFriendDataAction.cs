namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;

public class DeleteFriendDataAction : IFriendAction {
    private IFriendRepository friendRepository;

    public string InternalName => "Action_DeleteData";
    public FontAwesomeIcon Icon => FontAwesomeIcon.TrashAlt;

    public DeleteFriendDataAction(IFriendRepository friendRepository) {
        this.friendRepository = friendRepository;
    }

    public bool CanExecute(FriendProfile friend) {
        return friend.IsArchived;
    }

    public void Execute(FriendProfile friend) {
        this.friendRepository.RemoveFriendData(friend.ContentId);
    }
}