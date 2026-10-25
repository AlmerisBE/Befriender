namespace Befriender.Core.Configuration.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Proximity.Contracts;
using Dalamud.Game.ClientState.Conditions;
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
    private ICondition condition;

    private HashSet<Guid> pendingSourcesToRefresh = new();
    private DateTime? refreshTriggerTime = null;
    private bool fullRefreshPending = false;

    internal TimeSpan LoginDelay = TimeSpan.FromSeconds(12);
    internal TimeSpan TerritoryDelay = TimeSpan.FromSeconds(5);
    private readonly TimeSpan departureDelay = TimeSpan.FromSeconds(10);

    private Random random = new();
    private DateTime nextPeriodicSyncTime = DateTime.MaxValue;

    public AutomationService(
        IClientState clientState,
        IConfigurationService configService,
        ICharacterRegistry registry,
        IProximityService proximityService,
        IFramework framework,
        ICondition condition) {

        this.clientState = clientState;
        this.configService = configService;
        this.registry = registry;
        this.proximityService = proximityService;
        this.framework = framework;
        this.condition = condition;

        this.clientState.Login += this.OnLogin;
        this.clientState.TerritoryChanged += this.OnTerritoryChanged;
        this.proximityService.CharactersDeparted += this.OnCharactersDeparted;
        this.framework.Update += this.OnFrameworkUpdate;

        this.ScheduleNextPeriodicSync();
    }

    private void ScheduleNextPeriodicSync() {
        var config = this.configService.GetConfig();
        int min = config.MinSyncIntervalMinutes;
        int max = config.MaxSyncIntervalMinutes;

        if (min > max) min = max;

        int delayMinutes = this.random.Next(min, max + 1);
        this.nextPeriodicSyncTime = DateTime.Now.AddMinutes(delayMinutes);
    }

    private void OnLogin() {
        if (this.configService.GetConfig().SyncOnLogin) {
            this.fullRefreshPending = true;
            this.refreshTriggerTime = DateTime.Now.Add(this.LoginDelay);
        }
        this.ScheduleNextPeriodicSync();
    }

    private void OnTerritoryChanged(uint territoryId) {
        if (this.configService.GetConfig().SyncOnTerritoryChange) {
            this.fullRefreshPending = true;
            this.refreshTriggerTime = DateTime.Now.Add(this.TerritoryDelay);
        }
    }

    private void OnCharactersDeparted(IEnumerable<Character> characters) {
        if (!this.configService.GetConfig().SyncOnProximityDeparture) return;

        bool sourcesAdded = false;

        foreach (var character in characters) {
            if (character.ActiveSourceIds == null) continue;

            foreach (var sourceId in character.ActiveSourceIds) {
                if (this.pendingSourcesToRefresh.Add(sourceId)) sourcesAdded = true;
            }
        }

        if (sourcesAdded || this.pendingSourcesToRefresh.Count > 0) {
            if (!this.fullRefreshPending) {
                this.refreshTriggerTime = DateTime.Now.Add(this.departureDelay);
            }
        }
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (this.refreshTriggerTime.HasValue && DateTime.Now >= this.refreshTriggerTime.Value) {
            // Safely postpone the sync if the client is still transitioning (loading screens)
            if (this.condition[ConditionFlag.BetweenAreas] || this.condition[ConditionFlag.BetweenAreas51]) {
                this.refreshTriggerTime = DateTime.Now.AddSeconds(2);
                return;
            }

            this.refreshTriggerTime = null;

            if (this.fullRefreshPending) {
                this.fullRefreshPending = false;
                this.pendingSourcesToRefresh.Clear();
                this.registry.RequestRefresh(false);
            }
            else if (this.pendingSourcesToRefresh.Count > 0) {
                this.registry.RequestRefresh(this.pendingSourcesToRefresh.ToList(), false);
                this.pendingSourcesToRefresh.Clear();
            }
        }

        if (DateTime.Now >= this.nextPeriodicSyncTime) {
            if (this.condition[ConditionFlag.BetweenAreas] || this.condition[ConditionFlag.BetweenAreas51]) {
                this.nextPeriodicSyncTime = DateTime.Now.AddSeconds(10);
                return;
            }
            this.registry.RequestRefresh(false);
            this.ScheduleNextPeriodicSync();
        }
    }

    public void Dispose() {
        this.clientState.Login -= this.OnLogin;
        this.clientState.TerritoryChanged -= this.OnTerritoryChanged;
        this.proximityService.CharactersDeparted -= this.OnCharactersDeparted;
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}