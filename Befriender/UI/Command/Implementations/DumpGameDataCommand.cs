namespace Befriender.UI.Command.Implementations;

using Befriender.UI.Command.Contracts;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using System.Collections.Generic;

public class DumpGameDataCommand : ICommand {
    private IDataManager dataManager;
    private IPluginLog pluginLog;

    public string CommandTrigger => "dumpdata";
    public string Description => "Dumps OnlineStatus and TerritoryIntendedUse data to the plugin log for inspection.";

    public DumpGameDataCommand(IDataManager dataManager, IPluginLog pluginLog) {
        this.dataManager = dataManager;
        this.pluginLog = pluginLog;
    }

    public void Execute(string arguments) {
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

        this.pluginLog.Info("=== TERRITORY INTENDED USE ===");
        var territorySheet = this.dataManager.GetExcelSheet<TerritoryType>();
        if (territorySheet != null) {
            var documentedUses = new HashSet<uint>();
            foreach (var row in territorySheet) {
                var useId = row.TerritoryIntendedUse.RowId;

                // Track unique IntendedUse IDs and fetch one real-world PlaceName as an example
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
}