namespace Befriender.Tests.UI.Notifications.Services;

using Befriender.UI.Notifications.Services;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using NSubstitute;
using System;
using Xunit;

public class OnlineNotificationServiceTests {
    [Fact]
    public void OnCharacterLoggedOn_PrintsMessage_IfCharacterIsTracked() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockChatGui = Substitute.For<IChatGui>();
        var service = new OnlineNotificationService(mockRegistry, mockChatGui);

        var trackedChar = new Character { Name = "Alice", IsTrackedForNotifications = true };

        mockRegistry.CharacterLoggedOn += Raise.Event<Action<Character>>(trackedChar);

        mockChatGui.Received(1).Print(Arg.Any<SeString>());
    }

    [Fact]
    public void OnCharacterLoggedOn_IgnoresUntrackedCharacters() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockChatGui = Substitute.For<IChatGui>();
        var service = new OnlineNotificationService(mockRegistry, mockChatGui);

        var untrackedChar = new Character { Name = "Bob", IsTrackedForNotifications = false };

        mockRegistry.CharacterLoggedOn += Raise.Event<Action<Character>>(untrackedChar);

        mockChatGui.DidNotReceive().Print(Arg.Any<SeString>());
    }
}