namespace Befriender.Tests.Core.Proximity.Services;

using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Configuration.Contracts;
using global::Befriender.Core.Configuration.Models;
using global::Befriender.Core.Localization.Contracts;
using global::Befriender.Core.Proximity.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class ProximityServiceTests {
    [Fact]
    public void OnRegistryUpdated_Notifies_IfTrackedFriendIsNearby() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockConfig = Substitute.For<IConfigurationService>();
        var mockNotif = Substitute.For<INotificationManager>();
        var mockLoc = Substitute.For<ILocalizationService>();

        mockConfig.GetConfig().Returns(new PluginConfiguration { EnableProximityDetection = true, NotifyOnNearbyFriends = true });

        var service = new ProximityService(mockRegistry, mockConfig, mockNotif, mockLoc);

        var proximitySourceId = Guid.Parse("S1000000-0000-0000-0000-000000000003");
        var activeFriend = new Character { Id = Guid.NewGuid(), Name = "Alice" };
        activeFriend.ActiveSourceIds.Add(proximitySourceId);
        activeFriend.ActiveSourceIds.Add(Guid.NewGuid()); // Makes IsActivelyTracked = true

        mockRegistry.GetAllCharacters().Returns(new List<Character> { activeFriend });

        mockRegistry.RegistryUpdated += Raise.Event<Action>();

        mockNotif.Received(1).AddNotification(Arg.Any<Notification>());
        Assert.True(service.IsFriendNearby(activeFriend.ContentId));
    }
}