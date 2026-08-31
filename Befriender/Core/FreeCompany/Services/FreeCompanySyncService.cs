namespace Befriender.Core.FreeCompany.Services;

using Befriender.Core.FreeCompany.Contracts;
using Dalamud.Plugin.Services;
using System;

public class FreeCompanySyncService : IFreeCompanySyncService, IDisposable {
    private IFreeCompanyScanner scanner;
    private IFreeCompanyRepository repository;
    private IFramework framework;
    private bool isSyncActive = false;
    private DateTime lastSyncTime = DateTime.MinValue;
    private DateTime pendingSyncTime = DateTime.MaxValue;

    private DateTime dataStabilizedTime = DateTime.MaxValue;
    private int lastPolledCount = 0;

    private readonly TimeSpan syncInterval = TimeSpan.FromSeconds(60);

    public FreeCompanySyncService(IFreeCompanyScanner scanner, IFreeCompanyRepository repository, IFramework framework) {
        this.scanner = scanner;
        this.repository = repository;
        this.framework = framework;

        this.framework.Update += this.OnFrameworkUpdate;
    }

    public void StartSync() {
        if (!this.isSyncActive) {
            this.isSyncActive = true;
            this.RequestServerRefresh();
        }
    }

    public void StopSync() {
        this.isSyncActive = false;
    }

    public void RequestServerRefresh() {
        this.scanner.RequestServerUpdate();
        this.pendingSyncTime = DateTime.Now.AddSeconds(45); // Generous timeout for extremely large FCs
        this.dataStabilizedTime = DateTime.MaxValue;
        this.lastPolledCount = 0;
    }

    public void ForceSync(bool isFinalSync = true) {
        var members = this.scanner.ScanMembers();
        this.repository.UpdateMembers(members, isFinalSync);
        this.lastSyncTime = DateTime.Now;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (!this.isSyncActive) {
            return;
        }

        var now = DateTime.Now;

        if (this.pendingSyncTime != DateTime.MaxValue) {
            int currentCount = this.scanner.GetEntryCount();

            // When a new packet arrives from the server, we stream it to the UI and delay finalization
            if (currentCount > this.lastPolledCount) {
                this.lastPolledCount = currentCount;
                this.dataStabilizedTime = now.AddSeconds(5);

                this.ForceSync(false); // Stream partial data instantly to the interface
            }

            bool isStabilized = currentCount > 0 && now >= this.dataStabilizedTime;
            bool isTimedOut = now >= this.pendingSyncTime;

            if (isStabilized || isTimedOut) {
                if (currentCount > 0) {
                    this.ForceSync(true); // Finalize sync, allowing member cleanup
                }

                this.pendingSyncTime = DateTime.MaxValue;
                this.dataStabilizedTime = DateTime.MaxValue;
            }
        }
        else if (now - this.lastSyncTime >= this.syncInterval) {
            this.RequestServerRefresh();
        }
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}