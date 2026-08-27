namespace Befriender.UI.FriendList.Commands;

using Befriender.Core.Command.Contracts;
using Befriender.UI.FriendList.Contracts;
using System;

public class OpenFriendListCommand : ICommand {
    private IWindowNavigationService navService;

    public string CommandTrigger => string.Empty;
    public string Description => "Opens the Befriender Friend List UI.";

    public OpenFriendListCommand(IWindowNavigationService navService) {
        this.navService = navService;
    }

    public void Execute(string arguments) {
        if (arguments.Equals("config", StringComparison.OrdinalIgnoreCase)) {
            this.navService.OpenTab("Tab_Config");
        }
        else {
            this.navService.OpenTab("Tab_List");
        }
    }
}