namespace Befriender.Tests.Core.Actions.Implementations;

using Befriender.Core.Actions.Implementations;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.GameData.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class ArchivedFriendActionsTests {
    [Fact]
    public void EstateTeleportationAction_CanExecute_ReturnsFalseForArchivedFriends() {
        var action = new EstateTeleportationAction();
        var friend = new FriendProfile { IsArchived = true };

        Assert.False(action.CanExecute(friend));
    }

    [Fact]
    public void TrackFriendAction_CanExecute_ReturnsFalseForArchivedFriends() {
        var mockRepo = Substitute.For<IFriendRepository>();
        var action = new TrackFriendAction(mockRepo);
        var friend = new FriendProfile { IsArchived = true, IsTrackedForNotifications = false };

        Assert.False(action.CanExecute(friend));
    }

    [Fact]
    public void SendTellAction_CanExecute_ReturnsTrueForArchivedFriends() {
        var mockGameData = Substitute.For<IGameDataService>();
        var action = new SendTellAction(mockGameData);
        var friend = new FriendProfile { IsArchived = true, IsCharacterDeleted = false };

        Assert.True(action.CanExecute(friend));
    }

    [Fact]
    public void NativeInviteToPartyAction_CanExecute_ReturnsTrueForArchivedFriends() {
        var mockObjectTable = Substitute.For<IObjectTable>();
        var action = new NativeInviteToPartyAction(mockObjectTable);
        var friend = new FriendProfile { IsArchived = true, IsCharacterDeleted = false };

        Assert.True(action.CanExecute(friend));
    }
}