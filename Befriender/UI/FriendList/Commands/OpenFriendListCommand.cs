namespace Befriender.UI.FriendList.Commands;

using Befriender.Core.Command.Contracts;
using Befriender.UI.FriendList.Windows;

public class OpenFriendListCommand : ICommand {
    private FriendListWindow friendListWindow;

    public string CommandTrigger => string.Empty;
    public string Description => "Opens the Befriender Friend List UI.";

    public OpenFriendListCommand(FriendListWindow friendListWindow) {
        this.friendListWindow = friendListWindow;
    }

    public void Execute(string arguments) {
        this.friendListWindow.Toggle();
    }
}