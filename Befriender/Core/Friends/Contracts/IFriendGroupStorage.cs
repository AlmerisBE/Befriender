namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using System.Collections.Generic;

public interface IFriendGroupStorage {
    IReadOnlyList<FriendGroup> Load(string characterId);
    void Save(string characterId, IEnumerable<FriendGroup> groups);
}