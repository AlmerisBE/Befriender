namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Friends.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class FriendSyncServiceTests {
    [Fact]
    public void FriendSyncService_OnUpdate_PushesScannedFriendsWithStatusDetailsToRepository() {
        // Arrange
        var mockFramework = Substitute.For<IFramework>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockScanner = Substitute.For<IFriendScanner>();
        var mockRepository = Substitute.For<IFriendRepository>();
        var mockClientState = Substitute.For<IClientState>();

        // Update the mock to use the new randomized interval properties
        mockConfigService.GetConfig().Returns(new PluginConfiguration {
            MinSyncIntervalMinutes = 15,
            MaxSyncIntervalMinutes = 30
        });

        var dummyFriends = new List<FriendProfile> {
            new FriendProfile {
                ContentId = 12345,
                Name = "Test Friend",
                HomeWorldId = 33,
                IsOnline = true,
                JobId = 24,
                LocationId = 132,
                FcTag = "TEST"
            }
        };

        mockScanner.ScanActiveFriends().Returns(dummyFriends);

        using var service = new FriendSyncService(mockFramework, mockConfigService, mockScanner, mockRepository, mockClientState);

        // Act
        service.ForceSync();

        // Assert
        mockRepository.Received(1).UpdateFriends(Arg.Is<IEnumerable<FriendProfile>>(list =>
            list.First().IsOnline &&
            list.First().JobId == 24 &&
            list.First().FcTag == "TEST"
        ));
    }

    [Fact]
    public void RequestCrossWorldRefresh_EnforcesServerCooldown() {
        var mockFramework = Substitute.For<IFramework>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockScanner = Substitute.For<IFriendScanner>();
        var mockRepository = Substitute.For<IFriendRepository>();
        var mockClientState = Substitute.For<IClientState>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration());

        using var service = new FriendSyncService(mockFramework, mockConfigService, mockScanner, mockRepository, mockClientState);

        service.RequestCrossWorldRefresh();

        // Un deuxième appel immédiat devrait être bloqué par le cooldown de 5 à 10 secondes
        service.RequestCrossWorldRefresh();

        mockScanner.Received(1).RequestCrossWorldUpdate();
    }
}