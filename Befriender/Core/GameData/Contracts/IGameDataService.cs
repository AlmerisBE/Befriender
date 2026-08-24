namespace Befriender.Core.GameData.Contracts;

public interface IGameDataService {
    string GetWorldName(uint worldId);
    string GetJobAbbreviation(byte jobId);
    uint GetJobIconId(byte jobId);
    string GetLocationName(ushort territoryId);

    // We only expose the combined info method to the UI
    (uint IconId, string Name) GetOnlineStatusInfo(ulong stateMask);
}