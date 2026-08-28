namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using System;
using System.Collections.Generic;

public interface IFriendTagRepository {
    event Action? CacheCleared;
    IReadOnlyList<FriendTag> GetTags();
    void AddTag(string name);
    void UpdateTag(FriendTag tag);
    void RemoveTag(Guid id);
    void Save();
    void ClearCache();
}