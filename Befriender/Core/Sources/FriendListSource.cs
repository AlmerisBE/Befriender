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
    private List<Character> currentState = new();
    private bool isPolling = false;

    // Implementation of ICharacterSource
    public Guid SourceId { get; } = Guid.Parse("S1000000-0000-0000-0000-000000000001");
    public string Name => "FriendList";
    public int Priority => 10; // High priority for manual relationships

    public event Action? DataUpdated;

    public FriendListSource(IFriendListScanner scanner, IFramework framework) {
        this.scanner = scanner;
        this.framework = framework;
        this.framework.Update += this.OnFrameworkUpdate;
    }

    public IEnumerable<Character> GetCurrentState() {
        return this.currentState.ToList();
    }

    // Call this from the UI when the user explicitly requests a refresh
    public void TriggerManualRefresh() {
        this.scanner.RequestServerUpdate();
    }

    private void OnFrameworkUpdate(IFramework fw) {
        // Here we can limit the polling rate, but since GetStateHash is extremely fast
        // and doesn't allocate objects, we can run it safely every frame to ensure real-time updates.
        ulong currentHash = this.scanner.GetStateHash();

        if (currentHash != this.lastStateHash) {
            this.lastStateHash = currentHash;
            this.RefreshState();
        }
    }

    private void RefreshState() {
        var scannedCharacters = this.scanner.ScanActiveFriends().ToList();

        // Safety guard against uninitialized memory chunks returning 0 while we know we have friends
        if (scannedCharacters.Count == 0 && this.currentState.Count > 0) {
            // Check if the actual count reported by memory is also 0 to confirm deletion
            if (this.scanner.GetCurrentFriendCount() > 0) {
                return;
            }
        }

        this.currentState = scannedCharacters;
        this.DataUpdated?.Invoke();
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}