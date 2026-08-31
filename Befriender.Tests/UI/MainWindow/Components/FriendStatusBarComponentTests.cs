namespace Befriender.Tests.UI.MainWindow.Components;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.MainWindow.Components;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class FriendStatusBarComponentTests {
    [Fact]
    public void Constructor_SubscribesToRegistryUpdatedEvent() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockSources = new List<ICharacterSource>();
        var mockLoc = Substitute.For<ILocalizationService>();

        using var component = new FriendStatusBarComponent(mockRegistry, mockSources, mockLoc);

        mockRegistry.RegistryUpdated += Raise.Event<Action>();
        mockRegistry.Received(1).RegistryUpdated += Arg.Any<Action>();
    }
}