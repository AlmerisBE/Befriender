namespace Befriender.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Dalamud.Plugin.Services;

public class CharacterIdentityService : ICharacterIdentityService {
    private IObjectTable objectTable;

    public CharacterIdentityService(IObjectTable objectTable) {
        this.objectTable = objectTable;
    }

    public string GetCurrentCharacterId() {
        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return string.Empty;
        }

        var name = localPlayer.Name?.TextValue;
        if (string.IsNullOrEmpty(name)) {
            return string.Empty;
        }

        return $"{name}_{localPlayer.HomeWorld.RowId}";
    }
}