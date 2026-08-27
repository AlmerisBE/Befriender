namespace Befriender.Tests.UI.FriendList.Commands;

using Befriender.UI.FriendList.Commands;
using Befriender.UI.FriendList.Contracts;
using NSubstitute;
using Xunit;

public class OpenFriendListCommandTests {
    [Fact]
    public void Execute_WithConfigArgument_CallsOpenTabWithConfigInternalName() {
        // Arrange
        var mockNavService = Substitute.For<IWindowNavigationService>();
        var command = new OpenFriendListCommand(mockNavService);

        // Act
        command.Execute("config");

        // Assert
        mockNavService.Received(1).OpenTab("Tab_Config");
    }

    [Fact]
    public void Execute_WithEmptyArgument_CallsToggleWindow() {
        // Arrange
        var mockNavService = Substitute.For<IWindowNavigationService>();
        var command = new OpenFriendListCommand(mockNavService);

        // Act
        command.Execute(string.Empty);

        // Assert
        mockNavService.Received(1).ToggleWindow();
        mockNavService.DidNotReceiveWithAnyArgs().OpenTab(default!);
    }
}