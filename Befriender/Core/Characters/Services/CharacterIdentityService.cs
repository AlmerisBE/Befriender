namespace Befriender.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Dalamud.Plugin.Services;

public class CharacterIdentityService : ICharacterIdentityService {
    private IClientState clientState;
    private IObjectTable objectTable;

    public CharacterIdentityService(IClientState clientState, IObjectTable objectTable) {
        this.clientState = clientState;
        this.objectTable = objectTable;
    }

    public string GetCurrentCharacterId() {
        try {
            if (!this.clientState.IsLoggedIn) {
                return string.Empty;
            }

            var localPlayer = this.objectTable.LocalPlayer;
            if (localPlayer == null) {
                return string.Empty;
            }

            var name = localPlayer.Name.TextValue;
            if (string.IsNullOrWhiteSpace(name)) {
                return string.Empty;
            }

            return $"{name}_{localPlayer.HomeWorld.RowId}";
        }
        catch {
            return string.Empty;
        }
    }
}