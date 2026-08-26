namespace Befriender.Tests.Core.Notifications.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Notifications.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using System;
using Xunit;

public class OnlineNotificationServiceTests {
    [Fact]
    public void OnFriendLoggedOn_PrintsToChat_WhenFriendIsTracked() {
        // Arrange
        var mockRepo = Substitute.For<IFriendRepository>();
        var mockChat = Substitute.For<IChatGui>();
        using var service = new OnlineNotificationService(mockRepo, mockChat);

        var friend = new FriendProfile { Name = "Tracked Player", IsTrackedForNotifications = true };

        // Act
        mockRepo.FriendLoggedOn += Raise.Event<Action<FriendProfile>>(friend);

        // Assert
        mockChat.Received(1).Print(Arg.Is<Dalamud.Game.Text.SeStringHandling.SeString>(s => s.TextValue.Contains("Tracked Player is now online!")));
    }

    [Fact]
    public void OnFriendLoggedOn_DoesNothing_WhenFriendIsNotTracked() {
        // Arrange
        var mockRepo = Substitute.For<IFriendRepository>();
        var mockChat = Substitute.For<IChatGui>();
        using var service = new OnlineNotificationService(mockRepo, mockChat);

        var friend = new FriendProfile { Name = "Untracked Player", IsTrackedForNotifications = false };

        // Act
        mockRepo.FriendLoggedOn += Raise.Event<Action<FriendProfile>>(friend);

        // Assert
        mockChat.DidNotReceiveWithAnyArgs().Print(default(Dalamud.Game.Text.SeStringHandling.SeString)!);
    }
}