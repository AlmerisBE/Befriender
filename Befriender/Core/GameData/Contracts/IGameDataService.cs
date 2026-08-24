namespace Befriender.Core.GameData.Contracts;

public interface IGameDataService {
    string GetWorldName(uint worldId);
    string GetJobAbbreviation(byte jobId);
    uint GetJobIconId(byte jobId);
    string GetLocationName(ushort territoryId);

    (uint IconId, string Name) GetOnlineStatusInfo(ulong stateMask);

    bool IsFriendAvailable(ulong stateMask);
}