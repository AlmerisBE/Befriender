namespace Befriender.Tests.Core.Command.Services;

using Befriender.Core.Command.Services;
using Befriender.UI.Command.Contracts;
using Dalamud.Game.Command;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class CommandDispatcherTests {
    [Fact]
    public void CommandDispatcher_OnInitialization_RegistersMainCommands() {
        // Arrange
        var mockCommandManager = Substitute.For<ICommandManager>();
        var commands = new List<ICommand>();

        // Act
        using var dispatcher = new CommandDispatcher(mockCommandManager, commands);

        // Assert
        mockCommandManager.Received(1).AddHandler("/befriender", Arg.Any<CommandInfo>());
        mockCommandManager.Received(1).AddHandler("/fl", Arg.Any<CommandInfo>());
    }

    [Fact]
    public void CommandDispatcher_OnCommand_ExecutesDefaultCommandWhenNoArgsProvided() {
        // Arrange
        var mockCommandManager = Substitute.For<ICommandManager>();
        var mockDefaultCommand = Substitute.For<ICommand>();
        mockDefaultCommand.CommandTrigger.Returns(string.Empty);

        var commands = new List<ICommand> { mockDefaultCommand };
        CommandInfo capturedCommandInfo = null!;

        mockCommandManager.When(x => x.AddHandler(Arg.Any<string>(), Arg.Any<CommandInfo>()))
            .Do(callInfo => capturedCommandInfo = callInfo.Arg<CommandInfo>());

        using var dispatcher = new CommandDispatcher(mockCommandManager, commands);

        // Act
        capturedCommandInfo.Handler.Invoke("/fl", string.Empty);

        // Assert
        mockDefaultCommand.Received(1).Execute(string.Empty);
    }

    [Fact]
    public void CommandDispatcher_OnCommand_DispatchesToCorrectSubCommand() {
        // Arrange
        var mockCommandManager = Substitute.For<ICommandManager>();
        var mockConfigCommand = Substitute.For<ICommand>();
        mockConfigCommand.CommandTrigger.Returns("config");

        var commands = new List<ICommand> { mockConfigCommand };
        CommandInfo capturedCommandInfo = null!;

        mockCommandManager.When(x => x.AddHandler(Arg.Any<string>(), Arg.Any<CommandInfo>()))
            .Do(callInfo => capturedCommandInfo = callInfo.Arg<CommandInfo>());

        using var dispatcher = new CommandDispatcher(mockCommandManager, commands);

        // Act
        capturedCommandInfo.Handler.Invoke("/fl", "config extraArgs");

        // Assert
        mockConfigCommand.Received(1).Execute("extraArgs");
    }

    [Fact]
    public void CommandDispatcher_OnDispose_RemovesCommandRegistrations() {
        // Arrange
        var mockCommandManager = Substitute.For<ICommandManager>();
        var commands = new List<ICommand>();
        var dispatcher = new CommandDispatcher(mockCommandManager, commands);

        // Act
        dispatcher.Dispose();

        // Assert
        mockCommandManager.Received(1).RemoveHandler("/befriender");
        mockCommandManager.Received(1).RemoveHandler("/fl");
    }
}