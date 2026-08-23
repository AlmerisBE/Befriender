namespace Befriender.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using System.Collections.Generic;
using System.Linq;

public class FriendRepository : IFriendRepository {
    private List<FriendProfile> friends;
    private readonly object lockObj = new();
    private IFriendStorage storage;

    public FriendRepository(IFriendStorage storage) {
        this.storage = storage;
        this.friends = this.storage.Load().ToList();
    }

    public IReadOnlyList<FriendProfile> GetFriends() {
        lock (this.lockObj) {
            return this.friends.ToList();
        }
    }

    public void UpdateFriends(IEnumerable<FriendProfile> newFriends) {
        lock (this.lockObj) {
            this.friends = newFriends.ToList();
            this.storage.Save(this.friends);
        }
    }
}