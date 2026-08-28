namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using System.Collections.Generic;

public interface IFriendTagStorage {
    IEnumerable<FriendTag> Load(string characterId);
    void Save(string characterId, IEnumerable<FriendTag> tags);
}