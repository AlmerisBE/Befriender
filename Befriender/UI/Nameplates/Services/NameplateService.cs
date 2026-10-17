namespace Befriender.UI.Nameplates.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.UI.Nameplates.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class NameplateService : INameplateService, IDisposable {
    private INamePlateGui namePlateGui;
    private ICharacterRegistry registry;
    private IConfigurationService configService;

    public NameplateService(
        INamePlateGui namePlateGui,
        ICharacterRegistry registry,
        IConfigurationService configService) {

        this.namePlateGui = namePlateGui;
        this.registry = registry;
        this.configService = configService;

        this.namePlateGui.OnNamePlateUpdate += this.HandleNamePlateUpdate;
    }

    private void HandleNamePlateUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers) {
        var config = this.configService.GetConfig();
        if (!config.EnableOthersNameplateColor) return;

        foreach (var handler in handlers) {
            // The new API provides direct and safe access to the player character object
            var player = handler.PlayerCharacter;
            if (player == null) continue;

            var name = player.Name.TextValue;
            var worldId = player.HomeWorld.RowId;

            var characters = this.registry.GetAllCharacters();
            var match = characters.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.Ordinal) && c.HomeWorldId == worldId);

            // If the character is in the registry but NOT actively tracked, it belongs to "Others"
            if (match != null && !match.IsActivelyTracked) {
                uint colorU32 = ImGui.ColorConvertFloat4ToU32(config.OthersNameplateColor);

                // The new API simplifies color overriding by exposing TextColor directly
                handler.TextColor = colorU32;
            }
        }
    }

    public void Dispose() {
        this.namePlateGui.OnNamePlateUpdate -= this.HandleNamePlateUpdate;
    }
}