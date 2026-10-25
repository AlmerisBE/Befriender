namespace Befriender.Tests.Core.Sources.Friends;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Sources.Friends;
using Befriender.Core.Sources.Friends.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
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
    public void RequestRefresh_WithUserInitiatedTrue_ExecutesOpenAndCloseNativeList() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = false });
        var source = this.CreateSource();

        source.RequestRefresh(true);

        this.mockScanner.Received(1).OpenAndCloseNativeList();
        this.mockScanner.DidNotReceive().RequestServerUpdate();
        Assert.True(source.IsSyncing);
    }

    [Fact]
    public void RequestRefresh_WithUserInitiatedFalse_AndSyncOnLoginEnabled_OnFirstAttempt_ExecutesOpenAndCloseNativeList() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = true });
        var source = this.CreateSource();

        source.RequestRefresh(false);

        this.mockScanner.Received(1).OpenAndCloseNativeList();
        this.mockScanner.DidNotReceive().RequestServerUpdate();
    }

    [Fact]
    public void RequestRefresh_WithUserInitiatedFalse_AndSyncOnLoginEnabled_OnSubsequentAttempts_ExecutesRequestServerUpdate() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = true });
        var source = this.CreateSource();

        source.RequestRefresh(false); // First attempt (e.g., Login)
        source.RequestRefresh(false); // Second attempt (e.g., Zone change)

        this.mockScanner.Received(1).OpenAndCloseNativeList(); // Should only happen once
        this.mockScanner.Received(1).RequestServerUpdate();
    }

    [Fact]
    public void RequestRefresh_WithUserInitiatedFalse_AndSyncOnLoginDisabled_ExecutesRequestServerUpdate() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = false });
        var source = this.CreateSource();

        source.RequestRefresh(false);

        this.mockScanner.DidNotReceive().OpenAndCloseNativeList();
        this.mockScanner.Received(1).RequestServerUpdate();
    }
}