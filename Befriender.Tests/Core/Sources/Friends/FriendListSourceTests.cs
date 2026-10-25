namespace Befriender.Tests.Core.Sources.Friends;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Sources.Friends;
using Befriender.Core.Sources.Friends.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.Reflection;
using Xunit;

public class FriendListSourceTests {
    private IFriendListScanner mockScanner;
    private IFramework mockFramework;
    private IClientState mockClientState;
    private IConfigurationService mockConfigService;

    public FriendListSourceTests() {
        this.mockScanner = Substitute.For<IFriendListScanner>();
        this.mockFramework = Substitute.For<IFramework>();
        this.mockClientState = Substitute.For<IClientState>();
        this.mockConfigService = Substitute.For<IConfigurationService>();
    }

    private FriendListSource CreateSource() {
        return new FriendListSource(
            this.mockScanner,
            this.mockFramework,
            this.mockClientState,
            this.mockConfigService);
    }

    [Fact]
    public void RequestRefresh_WithUserInitiatedTrue_ExecutesToggleNativeListOnce_AndSetsFrameDelay() {
        // Arrange
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = false });
        var source = this.CreateSource();

        // Act
        source.RequestRefresh(true);

        // Assert
        this.mockScanner.Received(1).ToggleNativeList(); // Open execution
        this.mockScanner.DidNotReceive().RequestServerUpdate();
        Assert.True(source.IsSyncing);
    }

    [Fact]
    public void OnFrameworkUpdate_ExecutesToggleNativeListSecondTime_AfterFrameDelay() {
        // Arrange
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = true });
        var source = this.CreateSource();
        source.RequestRefresh(false); // Fires 1st toggle, sets 10 frames delay
        this.mockScanner.ClearReceivedCalls();

        // Act - Simulate 10 frames passing
        for (int i = 0; i < 10; i++) {
            source.GetType().GetMethod("OnFrameworkUpdate", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(source, new object[] { this.mockFramework });
        }

        // At this point, the delay should still be processed, we trigger the final frame
        source.GetType().GetMethod("OnFrameworkUpdate", BindingFlags.NonPublic | BindingFlags.Instance)?.Invoke(source, new object[] { this.mockFramework });

        // Assert
        this.mockScanner.Received(1).ToggleNativeList(); // Close execution
    }

    [Fact]
    public void RequestRefresh_WithUserInitiatedFalse_AndSyncOnLoginDisabled_ExecutesRequestServerUpdate() {
        // Arrange
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = false });
        var source = this.CreateSource();

        // Act
        source.RequestRefresh(false);

        // Assert
        this.mockScanner.DidNotReceive().ToggleNativeList();
        this.mockScanner.Received(1).RequestServerUpdate();
    }
}