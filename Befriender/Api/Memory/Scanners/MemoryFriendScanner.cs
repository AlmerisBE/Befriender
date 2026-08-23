namespace Befriender.Api.Memory.Scanners;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using System.Collections.Generic;
using System.Linq;

public class MemoryFriendScanner : IFriendScanner {
    public IEnumerable<FriendProfile> ScanActiveFriends() {
        // Here we will eventually map FFXIVClientStructs pointers (InfoProxyCrossRealm)
        // to FriendProfile objects.
        return Enumerable.Empty<FriendProfile>();
    }
}