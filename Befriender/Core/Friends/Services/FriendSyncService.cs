namespace Befriender.Core.Friends.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Friends.Contracts;
using Dalamud.Plugin.Services;
using System;

public class FriendSyncService : IFriendSyncService, IDisposable {
    private IFramework framework;
    private IConfigurationService configurationService;
    private IFriendScanner friendScanner;
    private IFriendRepository friendRepository;
    private IClientState clientState;

    private int lastFriendCount = -1;
    private DateTime pendingSyncTime = DateTime.MaxValue;

    public DateTime LastSyncTime { get; private set; } = DateTime.MinValue;

    public FriendSyncService(IFramework framework, IConfigurationService configurationService, IFriendScanner friendScanner, IFriendRepository friendRepository, IClientState clientState) {
        this.framework = framework;
        this.configurationService = configurationService;
        this.friendScanner = friendScanner;
        this.friendRepository = friendRepository;
        this.clientState = clientState;

        this.framework.Update += this.OnUpdate;
        this.clientState.Login += this.OnLogin;
        this.clientState.TerritoryChanged += this.OnTerritoryChanged;
    }

    private void OnLogin() {
        if (this.configurationService.GetConfig().SyncOnLogin) {
            this.ForceSync();
        }
    }

    private void OnTerritoryChanged(uint territoryId) {
        if (this.configurationService.GetConfig().SyncOnTerritoryChange) {
            this.ForceSync();
        }
    }

    private void OnUpdate(IFramework framework) {
        var config = this.configurationService.GetConfig();
        var now = DateTime.Now;

        if (config.SyncOnFriendListChange) {
            var currentCount = this.friendScanner.GetCurrentFriendCount();
            if (this.lastFriendCount != -1 && currentCount != this.lastFriendCount) {
                // Debounce: Wait 2 seconds for vanilla chunk loading to finish
                this.pendingSyncTime = now.AddSeconds(2);
            }
            this.lastFriendCount = currentCount;
        }

        var interval = TimeSpan.FromMinutes(config.SyncIntervalMinutes);
        if (now - this.LastSyncTime >= interval) {
            this.pendingSyncTime = now;
        }

        if (now >= this.pendingSyncTime) {
            this.pendingSyncTime = DateTime.MaxValue;
            this.ForceSync();
        }
    }

    public void ForceSync() {
        this.LastSyncTime = DateTime.Now;
        var scannedFriends = this.friendScanner.ScanActiveFriends();
        this.friendRepository.UpdateFriends(scannedFriends);
    }

    public void Dispose() {
        this.framework.Update -= this.OnUpdate;
        this.clientState.Login -= this.OnLogin;
        this.clientState.TerritoryChanged -= this.OnTerritoryChanged;
    }
}