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
    public DateTime LastSyncTime { get; private set; } = DateTime.MinValue;

    public FriendSyncService(IFramework framework, IConfigurationService configurationService, IFriendScanner friendScanner, IFriendRepository friendRepository) {
        this.framework = framework;
        this.configurationService = configurationService;
        this.friendScanner = friendScanner;
        this.friendRepository = friendRepository;

        this.framework.Update += this.OnUpdate;
    }

    private void OnUpdate(IFramework framework) {
        var config = this.configurationService.GetConfig();
        var interval = TimeSpan.FromMinutes(config.SyncIntervalMinutes);

        if (DateTime.Now - this.LastSyncTime >= interval) {
            this.LastSyncTime = DateTime.Now;
            var scannedFriends = this.friendScanner.ScanActiveFriends();
            this.friendRepository.UpdateFriends(scannedFriends);
        }
    }

    public void ForceSync() {
        this.LastSyncTime = DateTime.Now;
        var scannedFriends = this.friendScanner.ScanActiveFriends();
        this.friendRepository.UpdateFriends(scannedFriends);
    }

    public void Dispose() {
        this.framework.Update -= this.OnUpdate;
    }
}