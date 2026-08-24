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
    private IObjectTable objectTable;
    private string loadedCharacterId = string.Empty;

    public FriendRepository(IFriendStorage storage, ICharacterIdentityService identityService, IClientState clientState, IObjectTable objectTable) {
        this.storage = storage;
        this.identityService = identityService;
        this.clientState = clientState;
        this.objectTable = objectTable;
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
            if (string.IsNullOrEmpty(currentId)) {
                return;
            }

            this.EnsureLoaded();

            var currentTerritory = (ushort)this.clientState.TerritoryType;
            var now = DateTime.Now;
            var scannedList = scannedFriends.ToList();

            var repositoryDict = this.friends.ToDictionary(f => f.ContentId);

            // Build a fast lookup dictionary of players physically present around us
            var visiblePlayers = new Dictionary<(string, uint), Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>();
            foreach (var obj in this.objectTable) {
                if (obj is Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter player) {
                    visiblePlayers[(player.Name.TextValue, player.HomeWorld.RowId)] = player;
                }
            }

            foreach (var scanned in scannedList) {
                // Physical presence completely overrides offline proxy data
                if (visiblePlayers.TryGetValue((scanned.Name, scanned.HomeWorldId), out var presentPlayer)) {
                    scanned.IsOnline = true;
                    scanned.LocationId = currentTerritory;
                    scanned.JobId = (byte)presentPlayer.ClassJob.RowId;
                }

                if (repositoryDict.TryGetValue(scanned.ContentId, out var existing)) {
                    // Name change detection logic (US-2.1)
                    if (!string.Equals(existing.Name, scanned.Name, StringComparison.Ordinal) && !string.IsNullOrEmpty(existing.Name)) {
                        existing.PreviousNames ??= new List<string>();
                        if (!existing.PreviousNames.Contains(existing.Name)) {
                            existing.PreviousNames.Add(existing.Name);
                        }
                    }

                    existing.IsOnline = scanned.IsOnline;
                    existing.Name = scanned.Name;
                    existing.HomeWorldId = scanned.HomeWorldId;

                    if (scanned.IsOnline) {
                        existing.LastSeenAt = now;
                        existing.OnlineStateMask = scanned.OnlineStateMask;
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

            var scannedIds = scannedList.Select(f => f.ContentId).ToHashSet();
            foreach (var existing in repositoryDict.Values) {
                if (!scannedIds.Contains(existing.ContentId)) {
                    // Ultimate safeguard: if they are missing from proxy but physically rendered on screen, they are online!
                    if (visiblePlayers.TryGetValue((existing.Name, existing.HomeWorldId), out var presentPlayer)) {
                        existing.IsOnline = true;
                        existing.LocationId = currentTerritory;
                        existing.JobId = (byte)presentPlayer.ClassJob.RowId;
                        existing.LastSeenAt = now;
                    }
                    else {
                        existing.IsOnline = false;
                    }
                }
            }

            this.friends = repositoryDict.Values.ToList();
            this.storage.Save(this.loadedCharacterId, this.friends);
        }
    }

    public void Save() {
        lock (this.lockObj) {
            this.storage.Save(this.loadedCharacterId, this.friends);
        }
    }
}