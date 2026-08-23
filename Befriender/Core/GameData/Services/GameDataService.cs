namespace Befriender.Core.GameData.Services;

using Befriender.Core.GameData.Contracts;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

public class GameDataService : IGameDataService {
    private IDataManager dataManager;

    public GameDataService(IDataManager dataManager) {
        this.dataManager = dataManager;
    }

    public string GetWorldName(uint worldId) {
        if (worldId == 0) {
            return string.Empty;
        }

        var sheet = this.dataManager.GetExcelSheet<World>();
        if (sheet == null) {
            return worldId.ToString();
        }

        var row = sheet.GetRowOrDefault(worldId);
        return row.HasValue ? row.Value.Name.ToString() : worldId.ToString();
    }

    public string GetJobAbbreviation(byte jobId) {
        if (jobId == 0) {
            return string.Empty;
        }

        var sheet = this.dataManager.GetExcelSheet<ClassJob>();
        if (sheet == null) {
            return jobId.ToString();
        }

        var row = sheet.GetRowOrDefault(jobId);
        return row.HasValue ? row.Value.Abbreviation.ToString() : jobId.ToString();
    }

    public uint GetJobIconId(byte jobId) {
        if (jobId == 0) {
            return 0;
        }
        // In FFXIV UI, job icons start at 62100 offset by the jobId
        return 62100 + (uint)jobId;
    }

    public string GetLocationName(ushort territoryId) {
        if (territoryId == 0) {
            return string.Empty;
        }

        var territorySheet = this.dataManager.GetExcelSheet<TerritoryType>();
        if (territorySheet == null) {
            return territoryId.ToString();
        }

        var territoryRow = territorySheet.GetRowOrDefault(territoryId);
        if (territoryRow.HasValue && territoryRow.Value.PlaceName.RowId > 0) {
            var placeNameSheet = this.dataManager.GetExcelSheet<PlaceName>();
            if (placeNameSheet == null) {
                return territoryId.ToString();
            }

            var placeNameRow = placeNameSheet.GetRowOrDefault(territoryRow.Value.PlaceName.RowId);
            return placeNameRow.HasValue ? placeNameRow.Value.Name.ToString() : territoryId.ToString();
        }

        return territoryId.ToString();
    }
}