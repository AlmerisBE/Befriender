namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using System.Collections.Generic;

public interface IFriendStorage {
    IReadOnlyList<FriendProfile> Load();
    void Save(IEnumerable<FriendProfile> friends);
}