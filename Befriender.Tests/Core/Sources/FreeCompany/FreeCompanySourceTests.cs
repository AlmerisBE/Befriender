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
    public void RequestRefresh_WithUserInitiatedTrue_ExecutesOpenAndCloseNativeList() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { AutoSyncFreeCompanyOnLogin = false });
        var source = this.CreateSource();

        source.RequestRefresh(true);

        this.mockScanner.Received(1).OpenAndCloseNativeList();
        this.mockScanner.DidNotReceive().RequestServerUpdate();
        Assert.True(source.IsSyncing);
    }

    [Fact]
    public void RequestRefresh_WithUserInitiatedFalse_AndAutoSyncEnabled_OnFirstAttempt_ExecutesOpenAndCloseNativeList() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { AutoSyncFreeCompanyOnLogin = true });
        var source = this.CreateSource();

        source.RequestRefresh(false);

        this.mockScanner.Received(1).OpenAndCloseNativeList();
        this.mockScanner.DidNotReceive().RequestServerUpdate();
    }

    [Fact]
    public void RequestRefresh_WithUserInitiatedFalse_AndAutoSyncEnabled_OnSubsequentAttempts_ExecutesRequestServerUpdate() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { AutoSyncFreeCompanyOnLogin = true });
        var source = this.CreateSource();

        source.RequestRefresh(false); // First attempt (e.g., Login)
        source.RequestRefresh(false); // Second attempt (e.g., Zone change)

        this.mockScanner.Received(1).OpenAndCloseNativeList(); // Should only happen once
        this.mockScanner.Received(1).RequestServerUpdate(); // Fallback to silent update for subsequent calls
    }

    [Fact]
    public void RequestRefresh_WithUserInitiatedFalse_AndAutoSyncDisabled_ExecutesRequestServerUpdate() {
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { AutoSyncFreeCompanyOnLogin = false });
        var source = this.CreateSource();

        source.RequestRefresh(false);

        this.mockScanner.DidNotReceive().OpenAndCloseNativeList();
        this.mockScanner.Received(1).RequestServerUpdate();
    }
}