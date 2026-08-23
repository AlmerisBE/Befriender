namespace Befriender.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using System.Collections.Generic;
using System.Linq;

public class FriendRepository : IFriendRepository {
    private List<FriendProfile> friends = new();
    private readonly object lockObj = new();

    public IReadOnlyList<FriendProfile> GetFriends() {
        lock (this.lockObj) {
            return this.friends.ToList();
        }
    }

    public void UpdateFriends(IEnumerable<FriendProfile> newFriends) {
        lock (this.lockObj) {
            this.friends = newFriends.ToList();
        }
    }
}