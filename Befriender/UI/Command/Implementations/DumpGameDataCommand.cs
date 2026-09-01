namespace Befriender.UI.Command.Implementations;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.GameData.Contracts;
using Befriender.UI.Command.Contracts;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using System.Collections.Generic;

public class DumpGameDataCommand : ICommand {
    private IDataManager dataManager;
    private IPluginLog pluginLog;
    private IClientState clientState;
    private ICharacterRegistry registry;
    private IGameDataService gameDataService;

    public string CommandTrigger => "dumpdata";
    public string Description => "Dumps diagnostic data (status, territory, loc) to the plugin log.";

    public DumpGameDataCommand(
        IDataManager dataManager,
        IPluginLog pluginLog,
        IClientState clientState,
        ICharacterRegistry registry,
        IGameDataService gameDataService) {

        this.dataManager = dataManager;
        this.pluginLog = pluginLog;
        this.clientState = clientState;
        this.registry = registry;
        this.gameDataService = gameDataService;
    }

    public void Execute(string arguments) {
        string arg = arguments.Trim().ToLowerInvariant();

        if (arg == "status") {
            this.DumpOnlineStatus();
        }
        else if (arg == "territory") {
            this.DumpTerritoryUses();
        }
        else if (arg == "loc") {
            this.DumpLocationDiagnostics();
        }
        else {
            this.pluginLog.Info("Available arguments for /befriender dumpdata: status, territory, loc");
            this.DumpOnlineStatus();
            this.DumpTerritoryUses();
            this.DumpLocationDiagnostics();
        }
    }

    private void DumpOnlineStatus() {
        this.pluginLog.Info("=== ONLINE STATUS ICONS ===");
        var statusSheet = this.dataManager.GetExcelSheet<OnlineStatus>();
        if (statusSheet != null) {
            foreach (var row in statusSheet) {
                if (row.Icon == 0) {
                    continue;
                }

                this.pluginLog.Info($"Status ID: {row.RowId} | Icon: {row.Icon} | Name: {row.Name}");
            }
        }
    }

    private void DumpTerritoryUses() {
        this.pluginLog.Info("=== TERRITORY INTENDED USE ===");
        var territorySheet = this.dataManager.GetExcelSheet<TerritoryType>();
        if (territorySheet != null) {
            var documentedUses = new HashSet<uint>();
            foreach (var row in territorySheet) {
                var useId = row.TerritoryIntendedUse.RowId;

                if (useId > 0 && documentedUses.Add(useId)) {
                    var placeNameId = row.PlaceName.RowId;
                    string placeName = "Unknown";

                    if (placeNameId > 0) {
                        var placeNameSheet = this.dataManager.GetExcelSheet<PlaceName>();
                        if (placeNameSheet != null) {
                            var placeNameRow = placeNameSheet.GetRowOrDefault(placeNameId);
                            if (placeNameRow.HasValue) {
                                placeName = placeNameRow.Value.Name.ToString();
                            }
                        }
                    }
                    this.pluginLog.Info($"Use ID: {useId} | Example: {placeName}");
                }
            }
        }
    }

    private void DumpLocationDiagnostics() {
        uint currentTerritory = this.clientState.TerritoryType;
        string resolvedLocalName = this.gameDataService.GetLocationName(currentTerritory);

        this.pluginLog.Info("=== LOCATION DEBUG DUMP ===");
        this.pluginLog.Info($"LocalPlayer TerritoryType ID: {currentTerritory} | Resolved Name: '{resolvedLocalName}'");

        var characters = this.registry.GetAllCharacters();
        foreach (var c in characters) {
            if (!c.IsOnline) {
                continue;
            }

            string displayLocation = this.gameDataService.GetDisplayLocation(c.LocationId, c.CurrentWorldId, c.HomeWorldId, c.OnlineStateMask);
            string isTracked = c.IsActivelyTracked ? "Vanilla" : "Unsynchronized";

            this.pluginLog.Info($"- {c.Name} [{isTracked}] | LocationId: {c.LocationId} | Display: '{displayLocation}'");
        }

        this.pluginLog.Info("===========================");
    }
}