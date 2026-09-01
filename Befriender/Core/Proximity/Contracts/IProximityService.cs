namespace Befriender.Core.Proximity.Contracts;

using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;

public interface IProximityService {
    event Action<IEnumerable<Character>>? CharactersDeparted;

    bool IsFriendNearby(ulong contentId);
    IReadOnlyList<ulong> GetNearbyFriendIds();
}