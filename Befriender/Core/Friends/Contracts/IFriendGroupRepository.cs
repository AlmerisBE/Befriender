namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using System;
using System.Collections.Generic;

public interface IFriendGroupRepository {
    event Action? CacheCleared;
    IReadOnlyList<FriendGroup> GetGroups();
    void UpdateGroup(FriendGroup group);
    void Save();
    void ClearCache();
}