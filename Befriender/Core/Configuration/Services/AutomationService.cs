namespace Befriender.Core.Configuration.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Dalamud.Plugin.Services;
using System;

public class AutomationService : IDisposable {
    private IClientState clientState;
    private IConfigurationService configService;
    private ICharacterRegistry registry;

    public AutomationService(IClientState clientState, IConfigurationService configService, ICharacterRegistry registry) {
        this.clientState = clientState;
        this.configService = configService;
        this.registry = registry;

        this.clientState.Login += this.OnLogin;
        this.clientState.TerritoryChanged += this.OnTerritoryChanged;
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

    public void Dispose() {
        this.clientState.Login -= this.OnLogin;
        this.clientState.TerritoryChanged -= this.OnTerritoryChanged;
    }
}