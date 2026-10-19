namespace Befriender.Core.GameData.Services;

using Befriender.Core.GameData.Contracts;
using Dalamud.Plugin.Services;
using System;

public class LocalPlayerContext : ILocalPlayerContext, IDisposable {
    private IObjectTable objectTable;
    private IClientState clientState;
    private ulong cachedContentId;

    public LocalPlayerContext(IObjectTable objectTable, IClientState clientState) {
        this.objectTable = objectTable;
        this.clientState = clientState;

        this.clientState.Logout += this.OnLogout;
    }

    private void OnLogout(int type, int code) {
        this.cachedContentId = 0;
    }

    public ulong GetLocalContentId() {
        if (this.cachedContentId != 0) return this.cachedContentId;

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) return 0;

        // Generate a strictly deterministic 64-bit ID based on the globally unique Name + HomeWorld
        // This elegantly bypasses native memory layout volatility which yields garbage AccountIds.
        string identityKey = $"{localPlayer.Name.TextValue}@{localPlayer.HomeWorld.RowId}";
        this.cachedContentId = this.GenerateStableHash(identityKey);

        return this.cachedContentId;
    }

    private ulong GenerateStableHash(string str) {
        ulong hash = 14695981039346656037; // FNV offset basis
        foreach (char c in str) {
            hash ^= c;
            hash *= 1099511628211; // FNV prime
        }
        return hash;
    }

    public void Dispose() {
        this.clientState.Logout -= this.OnLogout;
    }
}