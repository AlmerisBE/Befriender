namespace Befriender.Tests.Core.Actions.Implementations;

using Befriender.Core.Actions.Implementations;
using Befriender.Core.Friends.Models;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class JoinFriendActionTests {
    [Fact]
    public void JoinFriendAction_CanExecute_ReturnsFalseWhenLocalPlayerIsNull() {
        // Arrange
        var mockObjectTable = Substitute.For<IObjectTable>();
        mockObjectTable.LocalPlayer.Returns((IPlayerCharacter)null!); // Simulate early load or disconnected state

        var mockDataManager = Substitute.For<IDataManager>();
        var mockLog = Substitute.For<IPluginLog>();

        var action = new JoinFriendAction(mockObjectTable, mockDataManager, mockLog);
        var friend = new FriendProfile { IsOnline = true, LocationId = 123, CurrentWorldId = 33 };

        // Act
        var result = action.CanExecute(friend);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void JoinFriendAction_CanExecute_ReturnsFalseWhenFriendIsOffline() {
        // Arrange
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockPlayer = Substitute.For<IPlayerCharacter>();
        mockObjectTable.LocalPlayer.Returns(mockPlayer);

        var mockDataManager = Substitute.For<IDataManager>();
        var mockLog = Substitute.For<IPluginLog>();

        var action = new JoinFriendAction(mockObjectTable, mockDataManager, mockLog);
        var friend = new FriendProfile { IsOnline = false, LocationId = 123, CurrentWorldId = 33 };

        // Act
        var result = action.CanExecute(friend);

        // Assert
        Assert.False(result);
    }
}