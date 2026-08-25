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
    private DateTime nextAutoSyncTime = DateTime.MaxValue;

    public DateTime LastSyncTime { get; private set; } = DateTime.MinValue;
    public bool IsSyncPending => this.pendingSyncTime != DateTime.MaxValue;
    public bool IsWindowOpen { get; set; } = false;

    public FriendSyncService(IFramework framework, IConfigurationService configurationService, IFriendScanner friendScanner, IFriendRepository friendRepository, IClientState clientState) {
        this.framework = framework;
        this.configurationService = configurationService;
        this.friendScanner = friendScanner;
        this.friendRepository = friendRepository;
        this.clientState = clientState;

        this.framework.Update += this.OnUpdate;
        this.clientState.Login += this.OnLogin;
        this.clientState.Logout += this.OnLogout;
        this.clientState.TerritoryChanged += this.OnTerritoryChanged;
    }

    private void OnLogin() {
        if (this.configurationService.GetConfig().SyncOnLogin) {
            this.ForceSync();
        }
    }

    private void OnLogout(int type, int code) {
        this.friendRepository.ClearCache();
        this.LastSyncTime = DateTime.MinValue;
        this.lastFriendCount = -1;
        this.lastStateHash = ulong.MaxValue;
        this.pendingSyncTime = DateTime.MaxValue;
    }

    private void OnTerritoryChanged(uint territoryId) {
        if (this.configurationService.GetConfig().SyncOnTerritoryChange) {
            this.ForceSync();
        }
    }

    public void RequestServerRefresh() {
        this.friendScanner.RequestServerUpdate();
        this.pendingSyncTime = DateTime.Now.AddSeconds(2);
        this.ScheduleNextAutoSync();
    }

    private void ScheduleNextAutoSync() {
        var config = this.configurationService.GetConfig();
        int min = Math.Max(5, config.MinSyncIntervalMinutes);
        int max = Math.Max(min + 15, config.MaxSyncIntervalMinutes);

        int randomMinutes = Random.Shared.Next(min, max + 1);
        this.nextAutoSyncTime = DateTime.Now.AddMinutes(randomMinutes);
    }

    private void OnUpdate(IFramework framework) {
        var config = this.configurationService.GetConfig();
        var now = DateTime.Now;

        if (config.SyncOnFriendListChange && now >= this.nextHashCheck) {
            this.nextHashCheck = now.AddSeconds(1);

            var currentCount = this.friendScanner.GetCurrentFriendCount();
            var currentHash = this.friendScanner.GetStateHash();

            if (this.lastFriendCount != -1 && (currentCount != this.lastFriendCount || currentHash != this.lastStateHash)) {
                this.pendingSyncTime = now.AddSeconds(2);
            }

            this.lastFriendCount = currentCount;
            this.lastStateHash = currentHash;
        }

        // Random automatic refresh ONLY if the window is currently open
        if (this.IsWindowOpen && now >= this.nextAutoSyncTime) {
            this.RequestServerRefresh();
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
        this.clientState.Logout -= this.OnLogout;
        this.clientState.TerritoryChanged -= this.OnTerritoryChanged;
    }
}