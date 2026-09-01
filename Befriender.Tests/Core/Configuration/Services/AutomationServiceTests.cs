namespace Befriender.Tests.Core.Configuration.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Configuration.Services;
using Befriender.Core.Proximity.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Threading;
using Xunit;

public class AutomationServiceTests {
    [Fact]
    public void OnLogin_TriggersRefresh_IfConfigured() {
        var mockClientState = Substitute.For<IClientState>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockProximity = Substitute.For<IProximityService>();
        var mockFramework = Substitute.For<IFramework>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = true });

        using var service = new AutomationService(mockClientState, mockConfigService, mockRegistry, mockProximity, mockFramework);

        mockClientState.Login += Raise.Event<Action>();

        mockRegistry.Received(1).RequestManualRefresh();
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

        mockRegistry.DidNotReceive().RequestManualRefresh();
    }

    [Fact]
    public void OnCharactersDeparted_AggregatesSources_AndRefreshesAfterDelay() {
        var mockClientState = Substitute.For<IClientState>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockProximity = Substitute.For<IProximityService>();
        var mockFramework = Substitute.For<IFramework>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnProximityDeparture = true });

        using var service = new AutomationService(mockClientState, mockConfigService, mockRegistry, mockProximity, mockFramework);

        var sourceA = Guid.NewGuid();
        var sourceB = Guid.NewGuid();

        var departingChars = new List<Character> {
            new Character { ActiveSourceIds = new HashSet<Guid> { sourceA } },
            new Character { ActiveSourceIds = new HashSet<Guid> { sourceB, sourceA } }
        };

        mockProximity.CharactersDeparted += Raise.Event<Action<IEnumerable<Character>>>(departingChars);

        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);
        mockRegistry.DidNotReceiveWithAnyArgs().RequestManualRefresh(default(IEnumerable<Guid>)!);

        Thread.Sleep(10100);
        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        mockRegistry.Received(1).RequestManualRefresh(Arg.Is<IEnumerable<Guid>>(ids =>
            ids.Contains(sourceA) && ids.Contains(sourceB)));
    }
}