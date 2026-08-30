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

    // We align the sync interval with the standard friend list pacing (e.g., 60 seconds)
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
            this.ForceSync();
        }
    }

    public void StopSync() {
        this.isSyncActive = false;
    }

    public void ForceSync() {
        var members = this.scanner.ScanMembers();
        this.repository.UpdateMembers(members);
        this.lastSyncTime = DateTime.Now;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (!this.isSyncActive) {
            return;
        }

        if (DateTime.Now - this.lastSyncTime >= this.syncInterval) {
            this.ForceSync();
        }
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}