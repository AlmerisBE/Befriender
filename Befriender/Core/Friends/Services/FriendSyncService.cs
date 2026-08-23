namespace Befriender.Core.Friends.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Friends.Contracts;
using Dalamud.Plugin.Services;
using System;

public class FriendSyncService : IDisposable {
    private IFramework framework;
    private IConfigurationService configurationService;
    private IFriendScanner friendScanner;
    private DateTime lastSyncTime = DateTime.MinValue;

    public FriendSyncService(IFramework framework, IConfigurationService configurationService, IFriendScanner friendScanner) {
        this.framework = framework;
        this.configurationService = configurationService;
        this.friendScanner = friendScanner;

        this.framework.Update += this.OnUpdate;
    }

    private void OnUpdate(IFramework framework) {
        var config = this.configurationService.GetConfig();
        var interval = TimeSpan.FromMinutes(config.SyncIntervalMinutes);

        if (DateTime.Now - this.lastSyncTime >= interval) {
            this.lastSyncTime = DateTime.Now;
            this.friendScanner.ScanActiveFriends();
        }
    }

    // Expose method for testing without reflecting private events
    public void TriggerUpdateForTesting() {
        this.OnUpdate(this.framework);
    }

    public void Dispose() {
        this.framework.Update -= this.OnUpdate;
    }
}