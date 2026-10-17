namespace Befriender.UI.Nameplates.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Logging.Contracts;
using Befriender.UI.Nameplates.Contracts;
using Dalamud.Game.Gui.NamePlate;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

public class NameplateService : INameplateService, IDisposable {
    private INamePlateGui namePlateGui;
    private ICharacterRegistry registry;
    private IConfigurationService configService;
    private ILoggerService logger;

    public NameplateService(
        INamePlateGui namePlateGui,
        ICharacterRegistry registry,
        IConfigurationService configService,
        ILoggerService logger) {

        this.namePlateGui = namePlateGui;
        this.registry = registry;
        this.configService = configService;
        this.logger = logger;

        this.logger.Info("[NameplateService] Initializing and subscribing to OnNamePlateUpdate.");
        this.namePlateGui.OnNamePlateUpdate += this.HandleNamePlateUpdate;
    }

    private void HandleNamePlateUpdate(INamePlateUpdateContext context, IReadOnlyList<INamePlateUpdateHandler> handlers) {
        var config = this.configService.GetConfig();
        if (!config.EnableOthersNameplateColor) return;

        var characters = this.registry.GetAllCharacters();

        using var leftSeString = new RentedSeStringBuilder();
        using var rightSeString = new RentedSeStringBuilder();

        leftSeString.Builder.PushColorRgba(config.OthersNameplateColor);
        rightSeString.Builder.PopColor();

        var wrapper = (
            leftSeString.Builder.ToReadOnlySeString().ToDalamudString(),
            rightSeString.Builder.ToReadOnlySeString().ToDalamudString()
        );

        foreach (var handler in handlers) {
            var player = handler.PlayerCharacter;
            if (player == null) continue;

            var name = player.Name.TextValue;
            var worldId = player.HomeWorld.RowId;

            var match = characters.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.Ordinal) && c.HomeWorldId == worldId);

            if (match != null && !match.IsActivelyTracked) handler.NameParts.TextWrap = wrapper;
        }
    }

    public void Dispose() {
        this.logger.Info("[NameplateService] Disposing and unsubscribing from OnNamePlateUpdate.");
        this.namePlateGui.OnNamePlateUpdate -= this.HandleNamePlateUpdate;
    }
}