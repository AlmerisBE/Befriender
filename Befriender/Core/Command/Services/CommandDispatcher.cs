namespace Befriender.Core.Command.Services;

using Befriender.Core.Command.Contracts;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class CommandDispatcher : IDisposable {
    private ICommandManager commandManager;
    private IEnumerable<ICommand> commands;
    private string[] mainCommands = { "/befriender", "/fl" };

    public CommandDispatcher(ICommandManager commandManager, IEnumerable<ICommand> commands) {
        this.commandManager = commandManager;
        this.commands = commands;

        foreach (var cmd in this.mainCommands) {
            this.commandManager.AddHandler(cmd, new CommandInfo(this.OnCommand) {
                HelpMessage = cmd == "/befriender" ? "Opens the friend list. Type '/befriender config' to access settings." : "Alias for /befriender"
            });
        }
    }

    private void OnCommand(string command, string arguments) {
        var args = arguments.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        var subCommand = args.Length > 0 ? args[0].ToLowerInvariant() : string.Empty;
        var subArguments = args.Length > 1 ? args[1] : string.Empty;

        var targetCommand = this.commands.FirstOrDefault(c => c.CommandTrigger.Equals(subCommand, StringComparison.InvariantCultureIgnoreCase));

        if (targetCommand == null && string.IsNullOrEmpty(subCommand)) {
            targetCommand = this.commands.FirstOrDefault(c => c.CommandTrigger == string.Empty);
        }

        if (targetCommand != null) {
            targetCommand.Execute(subArguments);
        }
    }

    public void Dispose() {
        foreach (var cmd in this.mainCommands) {
            this.commandManager.RemoveHandler(cmd);
        }
    }
}