namespace Befriender.Tests.Core.Actions.Implementations;

using Befriender.Core.Actions.Implementations;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using NSubstitute;
using Xunit;

public class DeleteFriendDataActionTests {
    [Fact]
    public void CanExecute_ReturnsTrueOnlyWhenArchived() {
        // Arrange
        var mockRepo = Substitute.For<IFriendRepository>();
        var action = new DeleteFriendDataAction(mockRepo);

        var activeFriend = new FriendProfile { IsArchived = false, IsCharacterDeleted = false };
        var activeDeletedChar = new FriendProfile { IsArchived = false, IsCharacterDeleted = true };
        var archivedFriend = new FriendProfile { IsArchived = true, IsCharacterDeleted = false };

        // Act & Assert
        Assert.False(action.CanExecute(activeFriend));
        Assert.False(action.CanExecute(activeDeletedChar)); // Should not allow deletion if still taking a vanilla slot
        Assert.True(action.CanExecute(archivedFriend));
    }

    [Fact]
    public void Execute_RemovesDataFromRepository() {
        // Arrange
        var mockRepo = Substitute.For<IFriendRepository>();
        var action = new DeleteFriendDataAction(mockRepo);
        var friend = new FriendProfile { ContentId = 123456, IsArchived = true };

        // Act
        action.Execute(friend);

        // Assert
        mockRepo.Received(1).RemoveFriendData(123456);
    }
}