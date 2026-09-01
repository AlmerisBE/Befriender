namespace Befriender.UI.MainWindow.Commands;

using Befriender.UI.Command.Contracts;
using Befriender.UI.MainWindow.Contracts;
using System;

public class OpenMainWindowCommand : ICommand {
    private IWindowNavigationService navService;

    public string CommandTrigger => string.Empty;
    public string Description => "Opens the Befriender Main Window.";

    public OpenMainWindowCommand(IWindowNavigationService navService) {
        this.navService = navService;
    }

    public void Execute(string arguments) {
        if (arguments.Equals("config", StringComparison.OrdinalIgnoreCase)) {
            this.navService.OpenTab("Tab_Config");
        }
        else {
            this.navService.ToggleWindow();
        }
    }
}