namespace Befriender.Core.Sources.Friends;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Sources.Friends.Contracts;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class FriendListSource : ICharacterSource, IDisposable {
    private IFriendListScanner scanner;
    private IFramework framework;

    private ulong lastStateHash = 0;
    private ulong pendingHash = 0;
    private DateTime dataStabilizedTime = DateTime.MaxValue;
    private List<Character> currentState = new();

    public Guid SourceId { get; } = Guid.Parse("51000000-0000-0000-0000-000000000001");
    public string Name => "FriendList";
    public int Priority => 10;

    public event Action? DataUpdated;

    public FriendListSource(IFriendListScanner scanner, IFramework framework) {
        this.scanner = scanner;
        this.framework = framework;
        this.framework.Update += this.OnFrameworkUpdate;
    }

    public IEnumerable<Character> GetCurrentState() {
        return this.currentState.ToList();
    }

    public void TriggerManualRefresh() {
        this.scanner.RequestServerUpdate();
    }

    private void OnFrameworkUpdate(IFramework fw) {
        ulong currentHash = this.scanner.GetStateHash();

        if (currentHash != this.lastStateHash) {
            if (currentHash != this.pendingHash) {
                this.pendingHash = currentHash;
                // Debounce: wait 1 second after the last memory shift to ensure data has fully streamed
                this.dataStabilizedTime = DateTime.Now.AddSeconds(1);
            }
        }
        else if (this.pendingHash != this.lastStateHash) {
            // Reverted back to the stable hash before the timer finished
            this.pendingHash = this.lastStateHash;
            this.dataStabilizedTime = DateTime.MaxValue;
        }

        if (this.dataStabilizedTime != DateTime.MaxValue && DateTime.Now >= this.dataStabilizedTime) {
            this.lastStateHash = this.pendingHash;
            this.dataStabilizedTime = DateTime.MaxValue;
            this.RefreshState();
        }
    }

    private void RefreshState() {
        var scannedCharacters = this.scanner.ScanActiveFriends().ToList();

        if (scannedCharacters.Count == 0 && this.currentState.Count > 0) {
            if (this.scanner.GetCurrentFriendCount() > 0) {
                return;
            }
        }

        this.currentState = scannedCharacters;
        this.DataUpdated?.Invoke();
    }

    public void RequestManualRefresh() {
        this.TriggerManualRefresh();
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}