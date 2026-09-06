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
    private IObjectTable objectTable;

    private ulong lastStateHash = 0;
    private ulong pendingHash = 0;
    private DateTime dataStabilizedTime = DateTime.MaxValue;
    private List<Character> currentState = new();
    private bool isManualRefreshPending = false;
    private DateTime lastSyncTime = DateTime.MinValue;
    private readonly TimeSpan syncInterval = TimeSpan.FromSeconds(60);

    public Guid SourceId { get; } = Guid.Parse("51000000-0000-0000-0000-000000000002");
    public string Name => "FreeCompany";
    public int Priority => 5;

    public bool IsSyncing => this.isManualRefreshPending || this.dataStabilizedTime != DateTime.MaxValue;

    public event Action? DataUpdated;

    public FreeCompanySource(IFreeCompanyScanner scanner, IFramework framework, IObjectTable objectTable) {
        this.scanner = scanner;
        this.framework = framework;
        this.objectTable = objectTable;
        this.framework.Update += this.OnFrameworkUpdate;
    }

    public IEnumerable<Character> GetCurrentState() {
        return this.currentState.ToList();
    }

    public void RequestManualRefresh() {
        this.TriggerManualRefresh();
    }

    public void TriggerManualRefresh() {
        this.isManualRefreshPending = true;
        this.lastSyncTime = DateTime.Now;
        this.scanner.RequestServerUpdate();
    }

    private void OnFrameworkUpdate(IFramework fw) {
        var now = DateTime.Now;

        // Periodic server request since Free Company data isn't actively pushed
        if (now - this.lastSyncTime >= this.syncInterval) {
            this.TriggerManualRefresh();
        }

        ulong currentHash = this.scanner.GetStateHash();

        if (currentHash != this.lastStateHash) {
            if (currentHash != this.pendingHash) {
                this.pendingHash = currentHash;
                this.dataStabilizedTime = now.AddSeconds(1); // Stabilization delay
            }
        }
        else if (this.pendingHash != this.lastStateHash) {
            this.pendingHash = this.lastStateHash;
            this.dataStabilizedTime = DateTime.MaxValue;
        }

        if (this.dataStabilizedTime != DateTime.MaxValue && now >= this.dataStabilizedTime) {
            this.lastStateHash = this.pendingHash;
            this.dataStabilizedTime = DateTime.MaxValue;
            this.RefreshState();
        }
    }

    private void RefreshState() {
        var scannedCharacters = this.scanner.ScanMembers().ToList();

        // The FFXIV client aggressively unloads the FreeCompanyMember proxy memory 
        // shortly after the data is received if the UI is not actively open.
        // If we receive an empty list but previously had data, we verify 
        // if the player is still in an FC via their CompanyTag before wiping our cache.
        if (scannedCharacters.Count == 0 && this.currentState.Count > 0) {
            var localPlayer = this.objectTable.LocalPlayer;
            bool hasFc = localPlayer != null && localPlayer.CompanyTag != null && !string.IsNullOrEmpty(localPlayer.CompanyTag.TextValue);

            if (hasFc) {
                this.isManualRefreshPending = false;
                return;
            }
        }

        this.currentState = scannedCharacters;
        this.isManualRefreshPending = false;
        this.DataUpdated?.Invoke();
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}