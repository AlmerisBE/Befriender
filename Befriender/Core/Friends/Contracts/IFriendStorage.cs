namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using System.Collections.Generic;

public interface IFriendStorage {
    IReadOnlyList<FriendProfile> Load(string characterId);
    void Save(string characterId, IEnumerable<FriendProfile> friends);
}