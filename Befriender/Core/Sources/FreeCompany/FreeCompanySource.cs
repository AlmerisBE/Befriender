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
    private IClientState clientState;

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

    public FreeCompanySource(IFreeCompanyScanner scanner, IFramework framework, IObjectTable objectTable, IClientState clientState) {
        this.scanner = scanner;
        this.framework = framework;
        this.objectTable = objectTable;
        this.clientState = clientState;

        this.framework.Update += this.OnFrameworkUpdate;
        this.clientState.Logout += this.OnLogout;
    }

    private void OnLogout(int type, int code) {
        this.currentState.Clear();
        this.lastStateHash = 0;
        this.pendingHash = 0;
        this.dataStabilizedTime = DateTime.MaxValue;
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

        if (now - this.lastSyncTime >= this.syncInterval) this.TriggerManualRefresh();

        ulong currentHash = this.scanner.GetStateHash();

        if (currentHash != this.lastStateHash) {
            if (currentHash != this.pendingHash) {
                this.pendingHash = currentHash;
                this.dataStabilizedTime = now.AddSeconds(1);
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

        if (scannedCharacters.Count == 0 && this.currentState.Count > 0) {
            var localPlayer = this.objectTable.LocalPlayer;
            bool hasFc = localPlayer != null && localPlayer.CompanyTag != null && !string.IsNullOrEmpty(localPlayer.CompanyTag.TextValue);

            if (hasFc) {
                this.isManualRefreshPending = false;
                return;
            }
        }

        var newState = new Dictionary<ulong, Character>();

        foreach (var c in this.currentState) {
            if (c.ContentId > 0) newState[c.ContentId] = c;
        }

        foreach (var scanned in scannedCharacters) {
            if (scanned.ContentId > 0) newState[scanned.ContentId] = scanned;
        }

        this.currentState = newState.Values.ToList();
        this.isManualRefreshPending = false;
        this.DataUpdated?.Invoke();
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
        this.clientState.Logout -= this.OnLogout;
    }
}