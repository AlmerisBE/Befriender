namespace Befriender.UI.Configuration.Commands;

using Befriender.Core.Command.Contracts;
using Befriender.UI.FriendList.Contracts;

public class ConfigCommand : ICommand {
    private IWindowNavigationService navService;

    public string CommandTrigger => "config";
    public string Description => "Opens the main interface and selects the configuration tab.";

    public ConfigCommand(IWindowNavigationService navService) {
        this.navService = navService;
    }

    public void Execute(string arguments) {
        this.navService.OpenTab("Tab_Config");
    }
}