namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using System;
using System.Collections.Generic;

public interface IFriendRepository {
    event Action CacheCleared;

    IReadOnlyList<FriendProfile> GetFriends();
    void UpdateFriends(IEnumerable<FriendProfile> friends);
    void RemoveFriendData(ulong contentId);
    void Save();
    void ClearCache();
}