namespace Befriender.Core.Configuration.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Proximity.Contracts;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class AutomationService : IDisposable {
    private IClientState clientState;
    private IConfigurationService configService;
    private ICharacterRegistry registry;
    private IProximityService proximityService;
    private IFramework framework;

    private HashSet<Guid> pendingSourcesToRefresh = new();
    private DateTime? refreshTriggerTime = null;
    private readonly TimeSpan departureDelay = TimeSpan.FromSeconds(10);

    public AutomationService(
        IClientState clientState,
        IConfigurationService configService,
        ICharacterRegistry registry,
        IProximityService proximityService,
        IFramework framework) {

        this.clientState = clientState;
        this.configService = configService;
        this.registry = registry;
        this.proximityService = proximityService;
        this.framework = framework;

        this.clientState.Login += this.OnLogin;
        this.clientState.TerritoryChanged += this.OnTerritoryChanged;
        this.proximityService.CharactersDeparted += this.OnCharactersDeparted;
        this.framework.Update += this.OnFrameworkUpdate;
    }

    private void OnLogin() {
        if (this.configService.GetConfig().SyncOnLogin) {
            this.registry.RequestManualRefresh();
        }
    }

    private void OnTerritoryChanged(uint territoryId) {
        if (this.configService.GetConfig().SyncOnTerritoryChange) {
            this.registry.RequestManualRefresh();
        }
    }

    private void OnCharactersDeparted(IEnumerable<Character> characters) {
        if (!this.configService.GetConfig().SyncOnProximityDeparture) {
            return;
        }

        bool sourcesAdded = false;

        foreach (var character in characters) {
            if (character.ActiveSourceIds == null) {
                continue;
            }

            foreach (var sourceId in character.ActiveSourceIds) {
                if (this.pendingSourcesToRefresh.Add(sourceId)) {
                    sourcesAdded = true;
                }
            }
        }

        if (sourcesAdded || this.pendingSourcesToRefresh.Count > 0) {
            this.refreshTriggerTime = DateTime.Now.Add(this.departureDelay);
        }
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (this.refreshTriggerTime.HasValue && DateTime.Now >= this.refreshTriggerTime.Value) {
            this.refreshTriggerTime = null;

            if (this.pendingSourcesToRefresh.Count > 0) {
                this.registry.RequestManualRefresh(this.pendingSourcesToRefresh.ToList());
                this.pendingSourcesToRefresh.Clear();
            }
        }
    }

    public void Dispose() {
        this.clientState.Login -= this.OnLogin;
        this.clientState.TerritoryChanged -= this.OnTerritoryChanged;
        this.proximityService.CharactersDeparted -= this.OnCharactersDeparted;
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}