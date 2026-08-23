namespace Befriender.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using System.Collections.Generic;
using System.Linq;

public class FriendRepository : IFriendRepository {
    private List<FriendProfile> friends = new();
    private readonly object lockObj = new();
    private IFriendStorage storage;
    private ICharacterIdentityService identityService;
    private string loadedCharacterId = string.Empty;

    public FriendRepository(IFriendStorage storage, ICharacterIdentityService identityService) {
        this.storage = storage;
        this.identityService = identityService;
    }

    private void EnsureLoaded() {
        var currentId = this.identityService.GetCurrentCharacterId();
        if (!string.IsNullOrEmpty(currentId) && this.loadedCharacterId != currentId) {
            this.friends = this.storage.Load(currentId).ToList();
            this.loadedCharacterId = currentId;
        }
    }

    public IReadOnlyList<FriendProfile> GetFriends() {
        lock (this.lockObj) {
            this.EnsureLoaded();
            return this.friends.ToList();
        }
    }

    public void UpdateFriends(IEnumerable<FriendProfile> newFriends) {
        lock (this.lockObj) {
            this.EnsureLoaded();
            this.friends = newFriends.ToList();

            if (!string.IsNullOrEmpty(this.loadedCharacterId)) {
                this.storage.Save(this.loadedCharacterId, this.friends);
            }
        }
    }
}