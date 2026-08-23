namespace Befriender.Core.Configuration.Commands;

using Befriender.Core.Command.Contracts;
using Befriender.UI.FriendList.Windows;

public class ConfigCommand : ICommand {
    private FriendListWindow mainWindow;

    public string CommandTrigger => "config";
    public string Description => "Ouvre l'interface principale (incluant la configuration).";

    public ConfigCommand(FriendListWindow mainWindow) {
        this.mainWindow = mainWindow;
    }

    public void Execute(string arguments) {
        this.mainWindow.IsOpen = true;
    }
}