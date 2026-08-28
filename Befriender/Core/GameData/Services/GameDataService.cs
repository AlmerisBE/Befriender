namespace Befriender.Core.GameData.Services;

using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using Lumina.Excel.Sheets;

public class GameDataService : IGameDataService {
    private IDataManager dataManager;
    private IObjectTable objectTable;
    private ILocalizationService loc;

    public GameDataService(IDataManager dataManager, IObjectTable objectTable, ILocalizationService loc) {
        this.dataManager = dataManager;
        this.objectTable = objectTable;
        this.loc = loc;
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
        if (territoryRow.HasValue) {
            uint placeNameId = territoryRow.Value.PlaceName.RowId;
            if (placeNameId == 0) {
                placeNameId = territoryRow.Value.PlaceNameZone.RowId;
            }

            if (placeNameId == 0) {
                placeNameId = territoryRow.Value.PlaceNameRegion.RowId;
            }

            if (placeNameId > 0) {
                var placeNameSheet = this.dataManager.GetExcelSheet<PlaceName>();
                if (placeNameSheet != null) {
                    var placeNameRow = placeNameSheet.GetRowOrDefault(placeNameId);
                    if (placeNameRow.HasValue) {
                        return placeNameRow.Value.Name.ToString();
                    }
                }
            }
        }
        return territoryId.ToString();
    }

    private bool IsInDutyTerritory(ushort territoryId) {
        if (territoryId == 0) {
            return false;
        }

        var sheet = this.dataManager.GetExcelSheet<TerritoryType>();
        if (sheet == null) {
            return false;
        }

        var row = sheet.GetRowOrDefault(territoryId);
        if (!row.HasValue) {
            return false;
        }

        return row.Value.TerritoryIntendedUse.RowId switch {
            3 or 4 or 8 or 10 or 18 or 26 or 27 or 28 or 29 or 31 or 33 or 34 or 36 or 37 or 38 or 39 or 41 or 46 or 47 or 48 or 52 or 53 or 54 or 56 or 57 or 58 or 59 or 60 or 61 or 63 => true,
            _ => false
        };
    }

    public bool IsCrossWorld(uint currentWorldId, uint homeWorldId, ulong stateMask, ushort locationId) {
        var state = (InfoProxyCommonList.CharacterData.OnlineStatus)stateMask;

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.InDuty) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.SharingDuty) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.SimilarDuty) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PvP) ||
            (stateMask != 0 && this.IsInDutyTerritory(locationId))) {
            return false;
        }

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return currentWorldId > 0 && currentWorldId != homeWorldId;
        }

        var localWorldId = localPlayer.CurrentWorld.RowId;
        var friendWorldId = currentWorldId > 0 ? currentWorldId : homeWorldId;

        return localWorldId != friendWorldId;
    }

    public string GetDisplayLocation(ushort locationId, uint currentWorldId, uint homeWorldId, ulong stateMask) {
        if (this.IsCrossWorld(currentWorldId, homeWorldId, stateMask, locationId)) {
            uint displayWorld = currentWorldId > 0 ? currentWorldId : homeWorldId;
            return this.GetWorldName(displayWorld);
        }

        var locationName = this.GetLocationName(locationId);

        if (string.IsNullOrEmpty(locationName) || locationName == locationId.ToString() || locationId == 0) {
            uint displayWorld = currentWorldId > 0 ? currentWorldId : homeWorldId;
            return this.GetWorldName(displayWorld);
        }

        return locationName;
    }

    private uint GetOnlineStatusRowId(ulong stateMask, ushort locationId) {
        if (stateMask == 0) {
            return 10;
        }

        var state = (InfoProxyCommonList.CharacterData.OnlineStatus)stateMask;

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PvP)) {
            return 13;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.InDuty)) {
            return 43;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AnotherWorld)) {
            return 40;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.SharingDuty)) {
            return 41;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.SimilarDuty)) {
            return 42;
        }

        if (this.IsInDutyTerritory(locationId)) {
            return 43;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.WaitingForDutyFinder)) {
            return 25;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.ViewingCutscene)) {
            return 15;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.CameraMode)) {
            return 18;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AwayFromKeyboard)) {
            return 17;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.Busy)) {
            return 12;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.RecruitingPartyMembers)) {
            return 26;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PlayingTripleTriad)) {
            return 14;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PartyLeaderCrossWorld)) {
            return 38;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PartyMemberCrossWorld)) {
            return 39;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AllianceLeader)) {
            return 33;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AlliancePartyLeader)) {
            return 34;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AlliancePartyMember)) {
            return 35;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PartyLeader)) {
            return 36;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PartyMember)) {
            return 37;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.RolePlaying)) {
            return 22;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.LookingForParty)) {
            return 23;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.LookingToMeldMateria)) {
            return 21;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.LookingForRepairs)) {
            return 19;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.LookingToRepair)) {
            return 20;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PvPMentor)) {
            return 30;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.TradeMentor)) {
            return 29;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PvEMentor)) {
            return 28;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.Mentor)) {
            return 27;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.BattleMentor)) {
            return 11;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.NewAdventurer)) {
            return 32;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.Returner)) {
            return 31;
        }

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.Online)) {
            return 47;
        }

        return 47;
    }

    public (uint IconId, string Name) GetOnlineStatusInfo(ulong stateMask, uint currentWorldId, uint homeWorldId, ushort locationId) {
        uint rowId = this.GetOnlineStatusRowId(stateMask, locationId);
        bool isCrossWorld = this.IsCrossWorld(currentWorldId, homeWorldId, stateMask, locationId);

        if (rowId == 10) {
            return (61504, this.loc.Translate("Status_Offline"));
        }

        if (rowId == 47 && isCrossWorld) {
            return (61505, this.loc.Translate("Status_CrossWorld"));
        }

        var sheet = this.dataManager.GetExcelSheet<OnlineStatus>();
        if (sheet != null) {
            var row = sheet.GetRowOrDefault(rowId);
            if (row.HasValue) {
                return (row.Value.Icon, row.Value.Name.ToString());
            }
        }

        return (61505, this.loc.Translate("Status_Unknown"));
    }

    public bool IsFriendAvailable(ulong stateMask) {
        var state = (InfoProxyCommonList.CharacterData.OnlineStatus)stateMask;

        if (state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AnotherWorld) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.InDuty) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.SharingDuty) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.SimilarDuty) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.PvP) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.ViewingCutscene) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.Busy) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.AwayFromKeyboard) ||
            state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.CameraMode)) {
            return false;
        }

        return true;
    }

    public string GetClientLanguageString(byte languageMask) {
        if (languageMask == 0) {
            return "None";
        }

        var languages = new System.Collections.Generic.List<string>();

        if ((languageMask & 1) != 0) {
            languages.Add("JA");
        }

        if ((languageMask & 2) != 0) {
            languages.Add("EN");
        }

        if ((languageMask & 4) != 0) {
            languages.Add("DE");
        }

        if ((languageMask & 8) != 0) {
            languages.Add("FR");
        }

        return languages.Count > 0 ? string.Join(" ", languages) : "None";
    }

    public uint GetGrandCompanyIconId(byte grandCompanyId) {
        return grandCompanyId switch {
            (byte)FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.Maelstrom => 60871,
            (byte)FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.TwinAdder => 60872,
            (byte)FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.ImmortalFlames => 60873,
            _ => 0
        };
    }

    public string GetGrandCompanyName(byte grandCompanyId) {
        if (grandCompanyId == 0) {
            return string.Empty;
        }

        var sheet = this.dataManager.GetExcelSheet<Lumina.Excel.Sheets.GrandCompany>();
        if (sheet == null) {
            return grandCompanyId.ToString();
        }

        var row = sheet.GetRowOrDefault(grandCompanyId);
        return row.HasValue ? row.Value.Name.ToString() : grandCompanyId.ToString();
    }
}