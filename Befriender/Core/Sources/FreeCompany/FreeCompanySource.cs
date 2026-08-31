namespace Befriender.Core.Sources.FreeCompany;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Sources.FreeCompany.Contracts;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class FreeCompanySource : ICharacterSource, IDisposable {
    private IFreeCompanyScanner scanner;
    private IFramework framework;

    private List<Character> currentState = new();
    private bool isSyncActive = false;
    private DateTime lastSyncTime = DateTime.MinValue;
    private DateTime pendingSyncTime = DateTime.MaxValue;
    private DateTime dataStabilizedTime = DateTime.MaxValue;
    private int lastPolledCount = 0;

    private readonly TimeSpan syncInterval = TimeSpan.FromSeconds(60);

    public Guid SourceId { get; } = Guid.Parse("51000000-0000-0000-0000-000000000002");
    public string Name => "FreeCompany";
    public int Priority => 5;

    public event Action? DataUpdated;

    public FreeCompanySource(IFreeCompanyScanner scanner, IFramework framework) {
        this.scanner = scanner;
        this.framework = framework;
        this.framework.Update += this.OnFrameworkUpdate;
    }

    public IEnumerable<Character> GetCurrentState() {
        return this.currentState.ToList();
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
        this.pendingSyncTime = DateTime.Now.AddSeconds(45);
        this.dataStabilizedTime = DateTime.MaxValue;
        this.lastPolledCount = 0;
    }

    private void ForceSync() {
        this.currentState = this.scanner.ScanMembers().ToList();
        this.lastSyncTime = DateTime.Now;
        this.DataUpdated?.Invoke();
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (!this.isSyncActive) {
            return;
        }

        var now = DateTime.Now;

        if (this.pendingSyncTime != DateTime.MaxValue) {
            int currentCount = this.scanner.GetEntryCount();

            if (currentCount > this.lastPolledCount) {
                this.lastPolledCount = currentCount;
                this.dataStabilizedTime = now.AddSeconds(5);
                this.ForceSync(); // Stream partial data instantly
            }

            bool isStabilized = currentCount > 0 && now >= this.dataStabilizedTime;
            bool isTimedOut = now >= this.pendingSyncTime;

            if (isStabilized || isTimedOut) {
                if (currentCount > 0) {
                    this.ForceSync();
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