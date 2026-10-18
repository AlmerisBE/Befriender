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
        // Return the cached ID immediately to prevent 0-value reads during zone transitions
        if (this.cachedContentId != 0) return this.cachedContentId;

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) return 0;

        unsafe {
            var csChar = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)localPlayer.Address;

            // Cache the value once it becomes valid (non-zero)
            if (csChar != null && csChar->AccountId != 0) {
                this.cachedContentId = csChar->AccountId;
            }
        }

        return this.cachedContentId;
    }

    public void Dispose() {
        this.clientState.Logout -= this.OnLogout;
    }
}