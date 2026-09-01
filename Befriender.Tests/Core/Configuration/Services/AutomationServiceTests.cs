namespace Befriender.Tests.Core.Configuration.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Configuration.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using System;
using Xunit;

public class AutomationServiceTests {
    [Fact]
    public void OnLogin_TriggersRefresh_IfConfigured() {
        var mockClientState = Substitute.For<IClientState>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = true });

        using var service = new AutomationService(mockClientState, mockConfigService, mockRegistry);

        // Correction : IClientState.Login utilise Action
        mockClientState.Login += Raise.Event<Action>();

        mockRegistry.Received(1).RequestManualRefresh();
    }

    [Fact]
    public void OnTerritoryChanged_IgnoresRefresh_IfNotConfigured() {
        var mockClientState = Substitute.For<IClientState>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnTerritoryChange = false });

        using var service = new AutomationService(mockClientState, mockConfigService, mockRegistry);

        // Correction : IClientState.TerritoryChanged utilise Action<uint>
        mockClientState.TerritoryChanged += Raise.Event<Action<uint>>(123u);

        mockRegistry.DidNotReceive().RequestManualRefresh();
    }
}