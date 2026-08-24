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

    public void UpdateFriends(IEnumerable<FriendProfile> scannedFriends) {
        lock (this.lockObj) {
            var currentId = this.identityService.GetCurrentCharacterId();

            // Guard: Do not process or wipe anything if the player is not fully logged in
            if (string.IsNullOrEmpty(currentId)) {
                return;
            }

            this.EnsureLoaded();

            var currentTerritory = (ushort)this.clientState.TerritoryType;
            var now = DateTime.Now;
            var scannedList = scannedFriends.ToList();

            // Fast lookup for existing friends to merge data
            var repositoryDict = this.friends.ToDictionary(f => f.ContentId);

            foreach (var scanned in scannedList) {
                if (repositoryDict.TryGetValue(scanned.ContentId, out var existing)) {
                    existing.IsOnline = scanned.IsOnline;
                    existing.Name = scanned.Name;
                    existing.HomeWorldId = scanned.HomeWorldId;

                    if (scanned.IsOnline) {
                        existing.LastSeenAt = now;
                    }

                    if (scanned.JobId > 0) {
                        existing.JobId = scanned.JobId;
                    }

                    if (scanned.LocationId > 0) {
                        existing.LocationId = scanned.LocationId;
                    }

                    if (!string.IsNullOrEmpty(scanned.FcTag)) {
                        existing.FcTag = scanned.FcTag;
                    }
                }
                else {
                    scanned.AddedAt = now;
                    scanned.AddedLocationId = currentTerritory;
                    scanned.LastSeenAt = scanned.IsOnline ? now : DateTime.MinValue;
                    repositoryDict[scanned.ContentId] = scanned;
                }
            }

            // Flag friends as offline if they exist in the repository but were missing from the scan
            var scannedIds = scannedList.Select(f => f.ContentId).ToHashSet();
            foreach (var existing in repositoryDict.Values) {
                if (!scannedIds.Contains(existing.ContentId)) {
                    existing.IsOnline = false;
                }
            }

            this.friends = repositoryDict.Values.ToList();
            this.storage.Save(this.loadedCharacterId, this.friends);
        }
    }
}