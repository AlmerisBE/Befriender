namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Friends.Models;
using System.Collections.Generic;

public interface IFriendScanner {
    IEnumerable<FriendProfile> ScanActiveFriends();
    int GetCurrentFriendCount();
}