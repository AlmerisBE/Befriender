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

        return $"{localPlayer.Name.TextValue}_{localPlayer.HomeWorld.RowId}";
    }
}