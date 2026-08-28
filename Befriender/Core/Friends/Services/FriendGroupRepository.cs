namespace Befriender.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class FriendGroupRepository : IFriendGroupRepository {
    private List<FriendGroup> groups = new();
    private readonly object lockObj = new();
    private IFriendGroupStorage storage;
    private ICharacterIdentityService identityService;
    private string loadedCharacterId = string.Empty;

    public event Action? CacheCleared;

    public FriendGroupRepository(IFriendGroupStorage storage, ICharacterIdentityService identityService) {
        this.storage = storage;
        this.identityService = identityService;
    }

    private void EnsureLoaded() {
        var currentId = this.identityService.GetCurrentCharacterId();
        if (!string.IsNullOrEmpty(currentId) && this.loadedCharacterId != currentId) {
            this.groups = this.storage.Load(currentId).ToList();

            // Seed vanilla FFXIV groups (0 = None, 1-7 = Symbols) if empty
            if (this.groups.Count == 0) {
                for (byte i = 0; i <= 7; i++) {
                    this.groups.Add(new FriendGroup { Id = i });
                }
            }

            this.loadedCharacterId = currentId;
        }
    }

    public IReadOnlyList<FriendGroup> GetGroups() {
        lock (this.lockObj) {
            this.EnsureLoaded();
            return this.groups.ToList();
        }
    }

    public void UpdateGroup(FriendGroup group) {
        lock (this.lockObj) {
            this.EnsureLoaded();
            var existing = this.groups.FirstOrDefault(g => g.Id == group.Id);
            if (existing != null) {
                existing.Title = group.Title;
                existing.Description = group.Description;
                this.Save();
            }
        }
    }

    public void Save() {
        lock (this.lockObj) {
            this.storage.Save(this.loadedCharacterId, this.groups);
        }
    }

    public void ClearCache() {
        lock (this.lockObj) {
            this.groups = new List<FriendGroup>();
            this.loadedCharacterId = string.Empty;
        }
        this.CacheCleared?.Invoke();
    }
}