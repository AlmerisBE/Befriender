namespace Befriender.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class FriendRepository : IFriendRepository {
    private List<FriendProfile> friends = new();
    private readonly object lockObj = new();
    private IFriendStorage storage;
    private ICharacterIdentityService identityService;
    private IClientState clientState;
    private string loadedCharacterId = string.Empty;

    public FriendRepository(IFriendStorage storage, ICharacterIdentityService identityService, IClientState clientState) {
        this.storage = storage;
        this.identityService = identityService;
        this.clientState = clientState;
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

            var incoming = newFriends.ToList();
            var currentTerritory = (ushort)this.clientState.TerritoryType;
            var now = DateTime.Now;

            foreach (var friend in incoming) {
                var existing = this.friends.FirstOrDefault(f => f.ContentId == friend.ContentId);

                if (existing != null) {
                    if (existing.AddedAt != DateTime.MinValue) {
                        friend.AddedAt = existing.AddedAt;
                        friend.AddedLocationId = existing.AddedLocationId;
                    }

                    // Preserve last known volatile data if the friend goes offline or if memory clears it
                    if (!friend.IsOnline) {
                        if (friend.JobId == 0 && existing.JobId != 0) {
                            friend.JobId = existing.JobId;
                        }

                        if (string.IsNullOrEmpty(friend.FcTag) && !string.IsNullOrEmpty(existing.FcTag)) {
                            friend.FcTag = existing.FcTag;
                        }

                        if (friend.LocationId == 0 && existing.LocationId != 0) {
                            friend.LocationId = existing.LocationId;
                        }
                    }
                }
                else {
                    friend.AddedAt = now;
                    friend.AddedLocationId = currentTerritory;
                }
            }

            this.friends = incoming;

            if (!string.IsNullOrEmpty(this.loadedCharacterId)) {
                this.storage.Save(this.loadedCharacterId, this.friends);
            }
        }
    }
}