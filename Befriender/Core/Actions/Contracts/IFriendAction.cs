namespace Befriender.Core.Actions.Contracts;

using Befriender.Core.Friends.Models;
using Dalamud.Interface;

public interface IFriendAction {
    string InternalName { get; }
    FontAwesomeIcon Icon { get; }

    bool CanExecute(FriendProfile friend);
    void Execute(FriendProfile friend);
}