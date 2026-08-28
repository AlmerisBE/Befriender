namespace Befriender.Tests.Core.Proximity.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Services;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class ProximityServiceTests {
    [Fact]
    public void OnFrameworkUpdate_IdentifiesNearbyFriendAndTriggersNotification() {
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockFramework = Substitute.For<IFramework>();
        var mockRepo = Substitute.For<IFriendRepository>();
        var mockConfig = Substitute.For<IConfigurationService>();
        var mockNotif = Substitute.For<INotificationManager>();
        var mockLoc = Substitute.For<ILocalizationService>();
        var mockClientState = Substitute.For<IClientState>();

        var config = new PluginConfiguration { EnableProximityDetection = true, NotifyOnNearbyFriends = true };
        mockConfig.GetConfig().Returns(config);

        // Utilisation de HomeWorldId = 0 pour correspondre à la structure Lumina par défaut
        var friend = new FriendProfile { ContentId = 123, Name = "Alice Liddell", HomeWorldId = 0, IsArchived = false };
        mockRepo.GetFriends().Returns(new List<FriendProfile> { friend });

        var mockPlayer = Substitute.For<IPlayerCharacter>();
        mockPlayer.Name.Returns(new SeString(new TextPayload("Alice Liddell")));

        var mockLocalPlayer = Substitute.For<IPlayerCharacter>();
        mockLocalPlayer.Address.Returns(IntPtr.Zero);
        mockPlayer.Address.Returns(new IntPtr(1));
        mockObjectTable.LocalPlayer.Returns(mockLocalPlayer);

        var enumerator = new List<IGameObject> { mockPlayer }.GetEnumerator();
        mockObjectTable.GetEnumerator().Returns(enumerator);

        using var service = new ProximityService(mockObjectTable, mockFramework, mockRepo, mockConfig, mockNotif, mockLoc, mockClientState);

        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        Assert.True(service.IsFriendNearby(123));
        mockNotif.Received(1).AddNotification(Arg.Any<Notification>());
        mockRepo.Received(1).UpdateFriendFromCharacter(123, mockPlayer, Arg.Any<ushort>());
    }

    [Fact]
    public void OnFrameworkUpdate_DoesNotIdentifyIfProximityDetectionIsDisabled() {
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockFramework = Substitute.For<IFramework>();
        var mockRepo = Substitute.For<IFriendRepository>();
        var mockConfig = Substitute.For<IConfigurationService>();
        var mockNotif = Substitute.For<INotificationManager>();
        var mockLoc = Substitute.For<ILocalizationService>();
        var mockClientState = Substitute.For<IClientState>();

        var config = new PluginConfiguration { EnableProximityDetection = false };
        mockConfig.GetConfig().Returns(config);

        using var service = new ProximityService(mockObjectTable, mockFramework, mockRepo, mockConfig, mockNotif, mockLoc, mockClientState);

        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        Assert.Empty(service.GetNearbyFriendIds());
        // Correction CS8625: Utilisation de Arg.Any<Notification>() au lieu de default
        mockNotif.DidNotReceiveWithAnyArgs().AddNotification(Arg.Any<Notification>());
    }
}