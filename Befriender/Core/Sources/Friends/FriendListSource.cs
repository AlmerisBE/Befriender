namespace Befriender.Core.Sources.Friends;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Sources.Friends.Contracts;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class FriendListSource : ICharacterSource, IDisposable {
    private IFriendListScanner scanner;
    private IFramework framework;
    private IClientState clientState;
    private IConfigurationService configService;

    private ulong lastStateHash = 0;
    private ulong pendingHash = 0;
    private DateTime dataStabilizedTime = DateTime.MaxValue;
    private DateTime timeoutTime = DateTime.MaxValue;

    private List<Character> currentState = new();
    private bool isManualRefreshPending = false;
    private bool hasAttemptedAutoOpenThisSession = false;

    // Frame delay variables for native UI toggling
    private bool isNativeListOpen = false;
    private int nativeListCloseFrameDelay = 0;

    public Guid SourceId { get; } = Guid.Parse("51000000-0000-0000-0000-000000000001");
    public string Name => "FriendList";
    public int Priority => 10;

    public bool IsSyncing => this.isManualRefreshPending || this.dataStabilizedTime != DateTime.MaxValue || this.isNativeListOpen;

    public event Action? DataUpdated;

    public FriendListSource(IFriendListScanner scanner, IFramework framework, IClientState clientState, IConfigurationService configService) {
        this.scanner = scanner;
        this.framework = framework;
        this.clientState = clientState;
        this.configService = configService;

        this.framework.Update += this.OnFrameworkUpdate;
        this.clientState.Logout += this.OnLogout;
    }

    private void OnLogout(int type, int code) {
        this.currentState.Clear();
        this.lastStateHash = 0;
        this.pendingHash = 0;
        this.dataStabilizedTime = DateTime.MaxValue;
        this.timeoutTime = DateTime.MaxValue;
        this.isManualRefreshPending = false;
        this.hasAttemptedAutoOpenThisSession = false;
        this.isNativeListOpen = false;
    }

    public IEnumerable<Character> GetCurrentState() {
        return this.currentState.ToList();
    }

    public void RequestRefresh(bool isUserInitiated) {
        this.isManualRefreshPending = true;
        this.timeoutTime = DateTime.Now.AddSeconds(5);

        var config = this.configService.GetConfig();
        bool shouldOpenNativeUI = false;

        if (isUserInitiated) {
            shouldOpenNativeUI = true;
        }
        else if (!this.hasAttemptedAutoOpenThisSession && config.SyncOnLogin) {
            this.hasAttemptedAutoOpenThisSession = true;
            shouldOpenNativeUI = true;
        }

        if (shouldOpenNativeUI) {
            this.scanner.ToggleNativeList(); // First execution (Open)
            this.isNativeListOpen = true;
            this.nativeListCloseFrameDelay = 10; // Wait 10 frames before closing
        }
        else {
            this.scanner.RequestServerUpdate();
        }

        this.DataUpdated?.Invoke();
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (this.isNativeListOpen) {
            if (this.nativeListCloseFrameDelay > 0) {
                this.nativeListCloseFrameDelay--;
            }
            else {
                this.isNativeListOpen = false;
                this.scanner.ToggleNativeList(); // Second execution (Close)
            }
        }

        var now = DateTime.Now;
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
            this.timeoutTime = DateTime.MaxValue;
            this.RefreshState();
        }
        else if (this.isManualRefreshPending && now >= this.timeoutTime) {
            this.timeoutTime = DateTime.MaxValue;
            this.RefreshState();
        }
    }

    private void RefreshState() {
        this.isManualRefreshPending = false;
        var scannedCharacters = this.scanner.ScanActiveFriends().ToList();

        if (scannedCharacters.Count == 0 && this.currentState.Count > 0) {
            if (this.scanner.GetCurrentFriendCount() > 0) {
                this.DataUpdated?.Invoke();
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
        this.DataUpdated?.Invoke();
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
        this.clientState.Logout -= this.OnLogout;
    }
}