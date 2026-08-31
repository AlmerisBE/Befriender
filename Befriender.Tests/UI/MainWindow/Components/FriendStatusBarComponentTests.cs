namespace Befriender.Tests.UI.MainWindow.Components;

using Befriender.UI.Localization.Contracts;
using global::Befriender.Core.Characters.Contracts;
using global::Befriender.UI.MainWindow.Components;
using NSubstitute;
using System;
using Xunit;

public class FriendStatusBarComponentTests {
    [Fact]
    public void Constructor_SubscribesToRegistryUpdatedEvent() {
        // Arrange
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockLoc = Substitute.For<ILocalizationService>();

        // Act
        using var component = new FriendStatusBarComponent(mockRegistry, mockLoc);

        // Simulate a registry update to verify event connectivity
        mockRegistry.RegistryUpdated += Raise.Event<Action>();

        // Assert
        // We ensure that the event subscription does not throw and is properly attached
        mockRegistry.Received(1).RegistryUpdated += Arg.Any<Action>();
    }
}