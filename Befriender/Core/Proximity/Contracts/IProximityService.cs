namespace Befriender.Core.Proximity.Contracts;

using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;
using System.Numerics;

public interface IProximityService {
    event Action<IEnumerable<Character>>? CharactersDeparted;

    bool IsFriendNearby(ulong contentId);
    IReadOnlyList<ulong> GetNearbyFriendIds();
    (float Distance, string CardinalDirectionKey)? GetSpatialRelation(Vector3 targetPosition);
}