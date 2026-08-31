namespace Befriender.Tests.Core.Proximity.Services;

using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Configuration.Contracts;
using global::Befriender.Core.Configuration.Models;
using global::Befriender.Core.Localization.Contracts;
using global::Befriender.Core.Proximity.Services;
using Lumina.Excel;
using Lumina.Excel.Sheets;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class ProximityServiceTests {
    [Fact]
    public void OnFrameworkUpdate_Notifies_IfTrackedFriendIsNearby() {
        // Arrange
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockConfig = Substitute.For<IConfigurationService>();
        var mockNotif = Substitute.For<INotificationManager>();
        var mockLoc = Substitute.For<ILocalizationService>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockFramework = Substitute.For<IFramework>();
        var mockClientState = Substitute.For<IClientState>();

        mockConfig.GetConfig().Returns(new PluginConfiguration { EnableProximityDetection = true, NotifyOnNearbyFriends = true });
        mockClientState.TerritoryType.Returns((ushort)123);

        var service = new ProximityService(mockRegistry, mockConfig, mockNotif, mockLoc, mockObjectTable, mockFramework, mockClientState);

        var trackedChar = new Character {
            Id = Guid.NewGuid(),
            Name = "Alice",
            HomeWorldId = 0,
            JobId = 99,
            Level = 10,
            IsOnline = false
        };
        trackedChar.ActiveSourceIds.Add(Guid.NewGuid());

        mockRegistry.GetAllCharacters().Returns(new List<Character> { trackedChar });

        var mockPlayer = Substitute.For<IPlayerCharacter>();
        mockPlayer.Name.Returns(new SeString(new TextPayload("Alice")));

        mockPlayer.HomeWorld.Returns(default(RowRef<World>));
        mockPlayer.CurrentWorld.Returns(default(RowRef<World>));
        mockPlayer.ClassJob.Returns(default(RowRef<ClassJob>));

        mockPlayer.Level.Returns((byte)90);

        mockObjectTable.Length.Returns(1);
        mockObjectTable[0].Returns(mockPlayer);

        var mockLocalPlayer = Substitute.For<IPlayerCharacter>();
        mockLocalPlayer.Address.Returns(123456);
        mockPlayer.Address.Returns(654321);
        mockObjectTable.LocalPlayer.Returns(mockLocalPlayer);

        // Act
        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        // Assert
        mockNotif.Received(1).AddNotification(Arg.Any<Notification>());
        Assert.True(service.IsFriendNearby(trackedChar.ContentId));

        Assert.Equal((byte)0, trackedChar.JobId);
        Assert.Equal((byte)90, trackedChar.Level);
        Assert.True(trackedChar.IsOnline);
        Assert.Equal(123u, trackedChar.LocationId);
    }
}