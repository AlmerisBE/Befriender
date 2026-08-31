namespace Befriender.Core.Sources.Friends.Contracts;

using Befriender.Core.Characters.Models;
using System.Collections.Generic;

public interface IFriendListScanner {
    IEnumerable<Character> ScanActiveFriends();
    int GetCurrentFriendCount();
    ulong GetStateHash();
    void RequestServerUpdate();
}