namespace Befriender.Core.GameData.Services;

using Befriender.Core.GameData.Contracts;
using Dalamud.Plugin.Services;

public class LocalPlayerContext : ILocalPlayerContext {
    private IObjectTable objectTable;

    public LocalPlayerContext(IObjectTable objectTable) {
        this.objectTable = objectTable;
    }

    public ulong GetLocalContentId() {
        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return 0;
        }

        unsafe {
            var csChar = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)localPlayer.Address;
            return csChar != null ? csChar->AccountId : 0;
        }
    }
}