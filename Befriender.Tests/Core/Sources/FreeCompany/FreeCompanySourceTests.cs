namespace Befriender.Tests.Core.Sources.FreeCompany;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Sources.FreeCompany;
using Befriender.Core.Sources.FreeCompany.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class FreeCompanySourceTests {
    private IFreeCompanyScanner mockScanner;
    private IFramework mockFramework;
    private IObjectTable mockObjectTable;
    private IClientState mockClientState;
    private IConfigurationService mockConfigService;

    public FreeCompanySourceTests() {
        this.mockScanner = Substitute.For<IFreeCompanyScanner>();
        this.mockFramework = Substitute.For<IFramework>();
        this.mockObjectTable = Substitute.For<IObjectTable>();
        this.mockClientState = Substitute.For<IClientState>();
        this.mockConfigService = Substitute.For<IConfigurationService>();
    }

    private FreeCompanySource CreateSource() {
        return new FreeCompanySource(
            this.mockScanner,
            this.mockFramework,
            this.mockObjectTable,
            this.mockClientState,
            this.mockConfigService);
    }

    [Fact]
    public void TriggerManualRefresh_CallsOpenAndClose_WhenConfigEnabledAndFirstAttempt() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { AutoSyncFreeCompanyOnLogin = true });
        var source = this.CreateSource();

        source.TriggerManualRefresh();

        this.mockScanner.Received(1).OpenAndCloseNativeList();
        this.mockScanner.DidNotReceive().RequestServerUpdate();
    }

    [Fact]
    public void TriggerManualRefresh_CallsRequestServerUpdate_OnSubsequentAttempts() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { AutoSyncFreeCompanyOnLogin = true });
        var source = this.CreateSource();

        source.TriggerManualRefresh(); // First attempt
        source.TriggerManualRefresh(); // Second attempt

        this.mockScanner.Received(1).OpenAndCloseNativeList();
        this.mockScanner.Received(1).RequestServerUpdate();
    }

    [Fact]
    public void TriggerManualRefresh_CallsRequestServerUpdate_WhenConfigDisabled() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { AutoSyncFreeCompanyOnLogin = false });
        var source = this.CreateSource();

        source.TriggerManualRefresh();

        this.mockScanner.DidNotReceive().OpenAndCloseNativeList();
        this.mockScanner.Received(1).RequestServerUpdate();
    }

    [Fact]
    public void TriggerManualRefresh_ExecutesToggleNativeListOnce_AndSetsFrameDelay() {
        // Arrange
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { AutoSyncFreeCompanyOnLogin = true });
        var source = this.CreateSource();

        // Act
        source.TriggerManualRefresh(isUserInitiated: true);

        // Assert
        this.mockScanner.Received(1).ToggleNativeList(); // Open execution
        Assert.True(source.IsSyncing);
    }

    [Fact]
    public void OnFrameworkUpdate_ExecutesToggleNativeListSecondTime_AfterFrameDelay() {
        // Arrange
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { AutoSyncFreeCompanyOnLogin = true });
        var source = this.CreateSource();
        source.TriggerManualRefresh(isUserInitiated: true); // Fires 1st toggle, sets 10 frames delay
        this.mockScanner.ClearReceivedCalls();

        // Act - Simulate 10 frames passing
        for (int i = 0; i < 10; i++) {
            source.GetType().GetMethod("OnFrameworkUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(source, new object[] { this.mockFramework });
        }

        // At this point, the delay should still be processed, we trigger the final frame
        source.GetType().GetMethod("OnFrameworkUpdate", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(source, new object[] { this.mockFramework });

        // Assert
        this.mockScanner.Received(1).ToggleNativeList(); // Close execution
    }
}