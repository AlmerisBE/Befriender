namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using System;
using System.Collections.Generic;

public interface IFriendGroupRepository {
    event Action? CacheCleared;
    IReadOnlyList<FriendGroup> GetGroups();
    void AddGroup(string title);
    void UpdateGroup(FriendGroup group);
    void RemoveGroup(Guid id);
    void MoveGroupUp(Guid id);
    void MoveGroupDown(Guid id);
    void Save();
    void ClearCache();
}