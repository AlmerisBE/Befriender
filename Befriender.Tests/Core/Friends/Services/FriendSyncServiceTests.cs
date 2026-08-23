namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class FriendSyncServiceTests {
    [Fact]
    public void FriendSyncService_OnFirstUpdate_TriggersScanner() {
        // Arrange
        var mockFramework = Substitute.For<IFramework>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockScanner = Substitute.For<IFriendScanner>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncIntervalMinutes = 15 });

        using var service = new FriendSyncService(mockFramework, mockConfigService, mockScanner);

        // Act
        // Simulate a framework tick by invoking the event handler
        service.TriggerUpdateForTesting();

        // Assert
        mockScanner.Received(1).ScanActiveFriends();
    }

    [Fact]
    public void FriendSyncService_OnConsecutiveUpdate_DoesNotTriggerScannerIfTimeNotElapsed() {
        // Arrange
        var mockFramework = Substitute.For<IFramework>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockScanner = Substitute.For<IFriendScanner>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncIntervalMinutes = 15 });

        using var service = new FriendSyncService(mockFramework, mockConfigService, mockScanner);

        // Act
        service.TriggerUpdateForTesting(); // First tick triggers it
        service.TriggerUpdateForTesting(); // Second tick immediately after should not

        // Assert
        // Still only 1 call received in total
        mockScanner.Received(1).ScanActiveFriends();
    }
}