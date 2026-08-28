namespace Befriender.Core.GameData.Contracts;

public interface IGameDataService {
    string GetWorldName(uint worldId);
    string GetJobAbbreviation(byte jobId);
    uint GetJobIconId(byte jobId);
    string GetLocationName(uint territoryId);

    bool IsCrossWorld(uint currentWorldId, uint homeWorldId, ulong stateMask, uint locationId);
    bool IsStandardTerritory(uint territoryId);
    string GetDisplayLocation(uint locationId, uint currentWorldId, uint homeWorldId, ulong stateMask);

    (uint IconId, string Name) GetOnlineStatusInfo(ulong stateMask, uint currentWorldId, uint homeWorldId, uint locationId);
    bool IsFriendAvailable(ulong stateMask);
    string GetClientLanguageString(byte languageMask);
    uint GetGrandCompanyIconId(byte grandCompanyId);
    string GetGrandCompanyName(byte grandCompanyId);

    string GetTitleName(ushort titleId, byte gender);
    string GetRaceName(byte raceId, byte gender);
    string GetTribeName(byte tribeId, byte gender);
}