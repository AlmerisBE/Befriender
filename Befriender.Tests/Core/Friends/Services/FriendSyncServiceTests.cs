namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Friends.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class FriendSyncServiceTests {
    [Fact]
    public void FriendSyncService_OnUpdate_PushesScannedFriendsToRepository() {
        // Arrange
        var mockFramework = Substitute.For<IFramework>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockScanner = Substitute.For<IFriendScanner>();
        var mockRepository = Substitute.For<IFriendRepository>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncIntervalMinutes = 15 });

        var dummyFriends = new List<FriendProfile> { new FriendProfile { Name = "Test Friend" } };
        mockScanner.ScanActiveFriends().Returns(dummyFriends);

        using var service = new FriendSyncService(mockFramework, mockConfigService, mockScanner, mockRepository);

        // Act
        service.TriggerUpdateForTesting();

        // Assert
        mockRepository.Received(1).UpdateFriends(dummyFriends);
    }
}