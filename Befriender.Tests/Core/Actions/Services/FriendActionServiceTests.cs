namespace Befriender.Tests.Core.Actions.Services;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Actions.Services;
using Befriender.Core.Friends.Models;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class FriendActionServiceTests {
    [Fact]
    public void FriendActionService_GetAvailableActions_ReturnsOnlyExecutableActionsForFriend() {
        // Arrange
        var mockActionOnlineOnly = Substitute.For<IFriendAction>();
        mockActionOnlineOnly.CanExecute(Arg.Is<FriendProfile>(f => f.IsOnline)).Returns(true);
        mockActionOnlineOnly.CanExecute(Arg.Is<FriendProfile>(f => !f.IsOnline)).Returns(false);

        var mockActionAlways = Substitute.For<IFriendAction>();
        mockActionAlways.CanExecute(Arg.Any<FriendProfile>()).Returns(true);

        var actions = new List<IFriendAction> { mockActionOnlineOnly, mockActionAlways };
        var service = new FriendActionService(actions);

        var onlineFriend = new FriendProfile { IsOnline = true };
        var offlineFriend = new FriendProfile { IsOnline = false };

        // Act
        var onlineAvailable = service.GetAvailableActions(onlineFriend);
        var offlineAvailable = service.GetAvailableActions(offlineFriend);

        // Assert
        Assert.Equal(2, onlineAvailable.Count);
        Assert.Single(offlineAvailable);
        Assert.Contains(mockActionAlways, offlineAvailable);
        Assert.DoesNotContain(mockActionOnlineOnly, offlineAvailable);
    }
}