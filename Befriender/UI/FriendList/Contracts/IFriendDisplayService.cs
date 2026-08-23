namespace Befriender.UI.FriendList.Contracts;

using Befriender.Core.Friends.Models;
using System.Collections.Generic;

public interface IFriendDisplayService {
    IReadOnlyList<FriendProfile> ProcessFriends(IEnumerable<FriendProfile> friends, bool showOnlineOnly, int sortColumnIndex, bool isAscending);
}