namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;

public class ArchiveFriendAction : IFriendAction {
    private IFriendRepository friendRepository;

    public string InternalName => "Action_Archive";
    public FontAwesomeIcon Icon => FontAwesomeIcon.Archive;

    public ArchiveFriendAction(IFriendRepository friendRepository) {
        this.friendRepository = friendRepository;
    }

    public bool CanExecute(FriendProfile friend) {
        return !friend.IsArchived && !friend.IsCharacterDeleted;
    }

    public void Execute(FriendProfile friend) {
        friend.IsArchived = true;
        this.friendRepository.Save();
    }
}