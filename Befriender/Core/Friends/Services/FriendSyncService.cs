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
    private ulong lastStateHash = ulong.MaxValue;
    private DateTime nextHashCheck = DateTime.MinValue;
    private DateTime pendingSyncTime = DateTime.MaxValue;

    public DateTime LastSyncTime { get; private set; } = DateTime.MinValue;
    public bool IsSyncPending => this.pendingSyncTime != DateTime.MaxValue;

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

        // 1. Throttle memory polling to once per second
        if (config.SyncOnFriendListChange && now >= this.nextHashCheck) {
            this.nextHashCheck = now.AddSeconds(1);

            var currentCount = this.friendScanner.GetCurrentFriendCount();
            var currentHash = this.friendScanner.GetStateHash();

            // Detect any variation in friend count OR any underlying status change
            if (this.lastFriendCount != -1 && (currentCount != this.lastFriendCount || currentHash != this.lastStateHash)) {
                this.pendingSyncTime = now.AddSeconds(2);
            }

            this.lastFriendCount = currentCount;
            this.lastStateHash = currentHash;
        }

        // 2. Interval fallback
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

    public void RequestServerRefresh() {
        this.friendScanner.RequestServerUpdate();

        // Sets a 2-second debounce. 
        // This provides visual feedback ("Scanning...") and acts as a fallback sync trigger 
        // in case the server responds but no statuses have actually changed (thus the hash remaining identical).
        this.pendingSyncTime = DateTime.Now.AddSeconds(2);
    }

    public void Dispose() {
        this.framework.Update -= this.OnUpdate;
        this.clientState.Login -= this.OnLogin;
        this.clientState.TerritoryChanged -= this.OnTerritoryChanged;
    }
}