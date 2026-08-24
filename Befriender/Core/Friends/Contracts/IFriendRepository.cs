namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using System.Collections.Generic;

public interface IFriendRepository {
    IReadOnlyList<FriendProfile> GetFriends();
    void UpdateFriends(IEnumerable<FriendProfile> friends);

    // Enables manual persistence of user-edited metadata (Notes, Archive state)
    void Save();
}