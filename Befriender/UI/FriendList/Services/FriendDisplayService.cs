namespace Befriender.UI.FriendList.Services;

using Befriender.Core.Friends.Models;
using Befriender.UI.FriendList.Contracts;
using System.Collections.Generic;
using System.Linq;

public class FriendDisplayService : IFriendDisplayService {
    public IReadOnlyList<FriendProfile> ProcessFriends(IEnumerable<FriendProfile> friends, bool showOnlineOnly, int sortColumnIndex, bool isAscending) {
        var query = friends.AsEnumerable();

        if (showOnlineOnly) {
            query = query.Where(f => f.IsOnline);
        }

        query = sortColumnIndex switch {
            0 => isAscending ? query.OrderBy(f => f.IsOnline).ThenBy(f => f.Name) : query.OrderByDescending(f => f.IsOnline).ThenBy(f => f.Name),
            1 => isAscending ? query.OrderBy(f => f.Name) : query.OrderByDescending(f => f.Name),
            2 => isAscending ? query.OrderBy(f => f.JobId).ThenBy(f => f.Name) : query.OrderByDescending(f => f.JobId).ThenBy(f => f.Name),
            3 => isAscending ? query.OrderBy(f => f.FcTag).ThenBy(f => f.Name) : query.OrderByDescending(f => f.FcTag).ThenBy(f => f.Name),
            4 => isAscending ? query.OrderBy(f => f.HomeWorldId).ThenBy(f => f.Name) : query.OrderByDescending(f => f.HomeWorldId).ThenBy(f => f.Name),
            _ => query
        };

        return query.ToList();
    }
}