namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using Dalamud.Game.ClientState.Objects.SubKinds;
using System;
using System.Collections.Generic;

public interface IFriendRepository {
    event Action CacheCleared;
    event Action<FriendProfile>? FriendLoggedOn;

    IReadOnlyList<FriendProfile> GetFriends();
    void UpdateFriends(IEnumerable<FriendProfile> friends);
    void UpdateFriendFromCharacter(ulong contentId, IPlayerCharacter player, uint territoryId);
    void RemoveFriendData(ulong contentId);
    void Save();
    void ClearCache();
}