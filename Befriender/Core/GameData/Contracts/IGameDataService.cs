namespace Befriender.Core.GameData.Contracts;

public interface IGameDataService {
    string GetWorldName(uint worldId);
    string GetJobAbbreviation(byte jobId);
    uint GetJobIconId(byte jobId);
    string GetLocationName(ushort territoryId);

    bool IsCrossWorld(uint currentWorldId, uint homeWorldId, ulong stateMask, ushort locationId);
    string GetDisplayLocation(ushort locationId, uint currentWorldId, uint homeWorldId, ulong stateMask);

    (uint IconId, string Name) GetOnlineStatusInfo(ulong stateMask, uint currentWorldId, uint homeWorldId, ushort locationId);
    bool IsFriendAvailable(ulong stateMask);
    string GetClientLanguageString(byte languageMask);
    uint GetGrandCompanyIconId(byte grandCompanyId);
    string GetGrandCompanyName(byte grandCompanyId);
}