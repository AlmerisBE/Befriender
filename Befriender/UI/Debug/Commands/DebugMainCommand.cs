namespace Befriender.UI.Debug.Commands;

using Befriender.UI.Command.Contracts;
using Befriender.UI.Debug.Windows;

public class DebugMainCommand : ICommand {
    private MainCommandDebugWindow debugWindow;

    public string CommandTrigger => "debug";
    public string Description => "Opens diagnostic tools (usage: /befriender debug cmds).";

    public DebugMainCommand(MainCommandDebugWindow debugWindow) {
        this.debugWindow = debugWindow;
    }

    public void Execute(string arguments) {
        string arg = arguments.Trim().ToLowerInvariant();

        if (arg == "cmds") {
            this.debugWindow.IsOpen = true;
        }
    }
}