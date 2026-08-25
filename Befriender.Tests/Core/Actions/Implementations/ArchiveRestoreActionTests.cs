namespace Befriender.Tests.Core.Actions.Implementations;

using Befriender.Core.Actions.Implementations;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using NSubstitute;
using Xunit;

public class ArchiveRestoreActionTests {
    [Fact]
    public void ArchiveFriendAction_CanExecute_ReturnsTrueOnlyWhenActive() {
        var mockRepo = Substitute.For<IFriendRepository>();
        var action = new ArchiveFriendAction(mockRepo);

        var activeFriend = new FriendProfile { IsArchived = false, IsCharacterDeleted = false };
        var archivedFriend = new FriendProfile { IsArchived = true, IsCharacterDeleted = false };
        var deletedFriend = new FriendProfile { IsArchived = false, IsCharacterDeleted = true };

        Assert.True(action.CanExecute(activeFriend));
        Assert.False(action.CanExecute(archivedFriend));
        Assert.False(action.CanExecute(deletedFriend));
    }

    [Fact]
    public void RestoreFriendAction_CanExecute_ReturnsTrueOnlyWhenArchived() {
        var mockRepo = Substitute.For<IFriendRepository>();
        var action = new RestoreFriendAction(mockRepo);

        var activeFriend = new FriendProfile { IsArchived = false, IsCharacterDeleted = false };
        var archivedFriend = new FriendProfile { IsArchived = true, IsCharacterDeleted = false };
        var deletedFriend = new FriendProfile { IsArchived = true, IsCharacterDeleted = true };

        Assert.False(action.CanExecute(activeFriend));
        Assert.True(action.CanExecute(archivedFriend));
        Assert.False(action.CanExecute(deletedFriend));
    }

    [Fact]
    public void ArchiveFriendAction_Execute_SetsArchivedAndSaves() {
        var mockRepo = Substitute.For<IFriendRepository>();
        var action = new ArchiveFriendAction(mockRepo);
        var friend = new FriendProfile { IsArchived = false };

        action.Execute(friend);

        Assert.True(friend.IsArchived);
        mockRepo.Received(1).Save();
    }

    [Fact]
    public void RestoreFriendAction_Execute_SetsActiveAndSaves() {
        var mockRepo = Substitute.For<IFriendRepository>();
        var action = new RestoreFriendAction(mockRepo);
        var friend = new FriendProfile { IsArchived = true };

        action.Execute(friend);

        Assert.False(friend.IsArchived);
        mockRepo.Received(1).Save();
    }
}