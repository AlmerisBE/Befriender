namespace Befriender.Tests.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Actions.Implementations;
using Befriender.Core.Friends.Models;
using NSubstitute;
using Xunit;

public class RequestRemoveFriendActionTests {
    [Fact]
    public void CanExecute_ReturnsTrueForUnarchivedFriendsIncludingDeleted() {
        // Arrange
        var mockService = Substitute.For<IRemoveFriendRequestService>();
        var action = new RequestRemoveFriendAction(mockService);

        var activeFriend = new FriendProfile { IsArchived = false, IsCharacterDeleted = false };
        var deletedVanillaFriend = new FriendProfile { IsArchived = false, IsCharacterDeleted = true };
        var archivedFriend = new FriendProfile { IsArchived = true, IsCharacterDeleted = false };

        // Act & Assert
        Assert.True(action.CanExecute(activeFriend));
        Assert.True(action.CanExecute(deletedVanillaFriend)); // Must be able to free vanilla slots!
        Assert.False(action.CanExecute(archivedFriend));
    }

    [Fact]
    public void Execute_FiresRequestRemovalOnService() {
        // Arrange
        var mockService = Substitute.For<IRemoveFriendRequestService>();
        var action = new RequestRemoveFriendAction(mockService);
        var friend = new FriendProfile { ContentId = 12345 };

        // Act
        action.Execute(friend);

        // Assert
        mockService.Received(1).RequestRemoval(friend);
    }
}