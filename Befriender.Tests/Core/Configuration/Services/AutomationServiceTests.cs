namespace Befriender.Tests.Core.Configuration.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Configuration.Services;
using Befriender.Core.Proximity.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
using System;
using System.Threading;
using Xunit;

public class AutomationServiceTests {
    [Fact]
    public void OnLogin_TriggersRefresh_IfConfigured_AfterDelay() {
        var mockClientState = Substitute.For<IClientState>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockProximity = Substitute.For<IProximityService>();
        var mockFramework = Substitute.For<IFramework>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = true });

        using var service = new AutomationService(mockClientState, mockConfigService, mockRegistry, mockProximity, mockFramework) {
            LoginDelay = TimeSpan.FromMilliseconds(50) // Micro-delay for test brevity
        };

        mockClientState.Login += Raise.Event<Action>();
        mockRegistry.DidNotReceive().RequestRefresh(false);

        Thread.Sleep(60);
        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        mockRegistry.Received(1).RequestRefresh(false);
    }

    [Fact]
    public void OnTerritoryChanged_IgnoresRefresh_IfNotConfigured() {
        var mockClientState = Substitute.For<IClientState>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockProximity = Substitute.For<IProximityService>();
        var mockFramework = Substitute.For<IFramework>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnTerritoryChange = false });

        using var service = new AutomationService(mockClientState, mockConfigService, mockRegistry, mockProximity, mockFramework);

        mockClientState.TerritoryChanged += Raise.Event<Action<uint>>(123u);

        mockRegistry.DidNotReceive().RequestRefresh(false);
    }

    [Fact]
    public void OnFrameworkUpdate_TriggersPeriodicSync_WhenTimeElapsed() {
        var mockClientState = Substitute.For<IClientState>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockProximity = Substitute.For<IProximityService>();
        var mockFramework = Substitute.For<IFramework>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { MinSyncIntervalMinutes = 0, MaxSyncIntervalMinutes = 0 });

        using var service = new AutomationService(mockClientState, mockConfigService, mockRegistry, mockProximity, mockFramework);

        Thread.Sleep(10);
        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        mockRegistry.Received(1).RequestRefresh(false);
    }
}