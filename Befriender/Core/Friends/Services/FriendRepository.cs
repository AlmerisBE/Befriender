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
    public event Action? CacheCleared;
    public event Action<FriendProfile>? FriendLoggedOn;

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
                if (visiblePlayers.TryGetValue((scanned.Name, scanned.HomeWorldId), out var presentPlayer)) {
                    scanned.IsOnline = true;
                    scanned.JobId = (byte)presentPlayer.ClassJob.RowId;
                }

                if (repositoryDict.TryGetValue(scanned.ContentId, out var existing)) {
                    bool isDeletedChar = string.IsNullOrWhiteSpace(scanned.Name);
                    bool wasOffline = !existing.IsOnline;

                    existing.IsCharacterDeleted = isDeletedChar;

                    if (!isDeletedChar) {
                        if (!string.Equals(existing.Name, scanned.Name, StringComparison.Ordinal) && !string.IsNullOrEmpty(existing.Name)) {
                            existing.PreviousNames ??= new List<string>();
                            if (!existing.PreviousNames.Contains(existing.Name)) {
                                existing.PreviousNames.Add(existing.Name);
                            }
                        }
                        existing.Name = scanned.Name;
                    }

                    existing.IsArchived = false;
                    existing.IsOnline = scanned.IsOnline;
                    existing.HomeWorldId = scanned.HomeWorldId;
                    existing.CurrentWorldId = scanned.CurrentWorldId;
                    existing.ClientLanguages = scanned.ClientLanguages;
                    existing.GrandCompany = scanned.GrandCompany;

                    if (scanned.IsOnline) {
                        existing.LastSeenAt = now;
                        existing.OnlineStateMask = scanned.OnlineStateMask;
                        existing.LocationId = scanned.LocationId;
                    }

                    if (scanned.JobId > 0) {
                        existing.JobId = scanned.JobId;
                    }

                    if (!string.IsNullOrEmpty(scanned.FcTag)) {
                        existing.FcTag = scanned.FcTag;
                    }

                    existing.GroupId = scanned.GroupId;

                    if (wasOffline && existing.IsOnline) {
                        this.FriendLoggedOn?.Invoke(existing);
                    }
                }
                else {
                    scanned.IsCharacterDeleted = string.IsNullOrWhiteSpace(scanned.Name);
                    scanned.AddedAt = now;
                    scanned.AddedLocationId = currentTerritory;
                    scanned.LastSeenAt = scanned.IsOnline ? now : DateTime.MinValue;
                    repositoryDict[scanned.ContentId] = scanned;
                }
            }

            var scannedIds = scannedList.Select(f => f.ContentId).ToHashSet();
            foreach (var existing in repositoryDict.Values) {
                if (!scannedIds.Contains(existing.ContentId)) {
                    existing.IsArchived = true;
                    existing.ArchivedAt = now;
                    existing.IsOnline = false;
                    existing.IsMarkedForRemoval = false;

                    if (visiblePlayers.TryGetValue((existing.Name, existing.HomeWorldId), out var presentPlayer)) {
                        existing.IsOnline = true;
                        // Retrait de existing.LocationId = currentTerritory;
                        existing.JobId = (byte)presentPlayer.ClassJob.RowId;
                        existing.LastSeenAt = now;
                    }
                }
            }

            this.friends = repositoryDict.Values.ToList();
            this.storage.Save(this.loadedCharacterId, this.friends);
        }
    }

    public void RemoveFriendData(ulong contentId) {
        lock (this.lockObj) {
            this.friends.RemoveAll(f => f.ContentId == contentId);
            this.Save();
        }
    }

    public void Save() {
        lock (this.lockObj) {
            this.storage.Save(this.loadedCharacterId, this.friends);
        }
    }

    public void ClearCache() {
        lock (this.lockObj) {
            this.friends = new List<FriendProfile>();
            this.loadedCharacterId = string.Empty;
        }

        this.CacheCleared?.Invoke();
    }
}