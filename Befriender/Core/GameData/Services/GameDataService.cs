namespace Befriender.Core.GameData.Services;

using Befriender.Core.GameData.Contracts;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
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

    private uint GetOnlineStatusIconId(ulong stateMask) {
        var state = (InfoProxyCommonList.CharacterData.OnlineStatus)stateMask;

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AnotherWorld)) {
            return 61535;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.InDuty)) {
            return 61510;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.WaitingForDutyFinder)) {
            return 61517;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.ViewingCutscene)) {
            return 61508;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.CameraMode)) {
            return 61546;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AwayFromKeyboard)) {
            return 61511;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.Busy)) {
            return 61509;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.RecruitingPartyMembers)) {
            return 61536;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PlayingTripleTriad)) {
            return 61539;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AllianceLeader)) {
            return 61518;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AlliancePartyLeader)) {
            return 61519;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AlliancePartyMember)) {
            return 61520;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PartyLeaderCrossWorld)) {
            return 61961;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PartyLeader)) {
            return 61521;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PartyMemberCrossWorld)) {
            return 61962;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PartyMember)) {
            return 61522;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.RolePlaying)) {
            return 61545;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.LookingForParty)) {
            return 61515;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.LookingToMeldMateria)) {
            return 61514;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.LookingForRepairs)) {
            return 61512;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.LookingToRepair)) {
            return 61513;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.Mentor)) {
            return 61540;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.BattleMentor)) {
            return 61542;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.TradeMentor)) {
            return 61543;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PvPMentor)) {
            return 61544;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.NewAdventurer)) {
            return 61523;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.Returner)) {
            return 61547;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.Online)) {
            return 61505;
        }

        return 61504; // Offline
    }

    public (uint IconId, string Name) GetOnlineStatusInfo(ulong stateMask) {
        var iconId = this.GetOnlineStatusIconId(stateMask);

        if (iconId == 0) {
            return (0, "Offline");
        }

        if (iconId == 61505) {
            return (61505, "Online");
        }

        var sheet = this.dataManager.GetExcelSheet<OnlineStatus>();
        if (sheet != null) {
            foreach (var row in sheet) {
                if (row.Icon == iconId) {
                    return (iconId, row.Name.ToString());
                }
            }
        }

        return (iconId, "Unknown");
    }
}