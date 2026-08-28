namespace Befriender.Core.Proximity.Contracts;

using System.Collections.Generic;

public interface IProximityService {
    bool IsFriendNearby(ulong contentId);
    IReadOnlyList<ulong> GetNearbyFriendIds();
}