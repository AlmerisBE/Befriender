namespace Befriender.Tests.UI.Debug.Commands;

using Befriender.Core.GameData.Contracts;
using Befriender.UI.Debug.Commands;
using Befriender.UI.Debug.Windows;
using NSubstitute;
using Xunit;

public class DebugMainCommandTests {
    [Fact]
    public void Execute_WithCmdsArgument_SetsDebugWindowIsOpenToTrue() {
        // Arrange
        var mockProvider = Substitute.For<IMainCommandProvider>();
        var window = new MainCommandDebugWindow(mockProvider);
        var command = new DebugMainCommand(window);

        // Act
        command.Execute("cmds");

        // Assert
        Assert.True(window.IsOpen);
    }

    [Fact]
    public void Execute_WithOtherArgument_DoesNotOpenWindow() {
        // Arrange
        var mockProvider = Substitute.For<IMainCommandProvider>();
        var window = new MainCommandDebugWindow(mockProvider);
        var command = new DebugMainCommand(window);

        // Act
        command.Execute("other");

        // Assert
        Assert.False(window.IsOpen);
    }
}