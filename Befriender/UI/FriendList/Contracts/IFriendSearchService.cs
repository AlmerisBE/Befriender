namespace Befriender.UI.FriendList.Contracts;

using Befriender.Core.Friends.Models;
using System.Collections.Generic;

public interface IFriendSearchService {
    IReadOnlyList<FriendProfile> FilterFriends(IEnumerable<FriendProfile> friends, string searchQuery);
}