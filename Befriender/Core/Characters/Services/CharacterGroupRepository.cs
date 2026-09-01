namespace Befriender.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterGroupRepository : ICharacterGroupRepository {
    private List<CharacterGroup> groups = new();
    private readonly object lockObj = new();
    private ICharacterGroupStorage storage;
    private ICharacterIdentityService identityService;
    private string loadedCharacterId = string.Empty;

    public event Action? CacheCleared;

    public CharacterGroupRepository(ICharacterGroupStorage storage, ICharacterIdentityService identityService) {
        this.storage = storage;
        this.identityService = identityService;
    }

    private void EnsureLoaded() {
        var currentId = this.identityService.GetCurrentCharacterId();
        if (!string.IsNullOrEmpty(currentId) && this.loadedCharacterId != currentId) {
            this.groups = this.storage.Load(currentId).ToList();
            this.loadedCharacterId = currentId;
        }
    }

    public IReadOnlyList<CharacterGroup> GetGroups() {
        lock (this.lockObj) {
            this.EnsureLoaded();
            return this.groups.ToList();
        }
    }

    public void AddGroup(string title) {
        lock (this.lockObj) {
            this.EnsureLoaded();
            this.groups.Add(new CharacterGroup { Title = title });
            this.Save();
        }
    }

    public void UpdateGroup(CharacterGroup group) {
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

    public void RemoveGroup(Guid id) {
        lock (this.lockObj) {
            this.EnsureLoaded();
            this.groups.RemoveAll(g => g.Id == id);
            this.Save();
        }
    }

    public void MoveGroupUp(Guid id) {
        lock (this.lockObj) {
            this.EnsureLoaded();
            var index = this.groups.FindIndex(g => g.Id == id);
            if (index > 0) {
                var group = this.groups[index];
                this.groups.RemoveAt(index);
                this.groups.Insert(index - 1, group);
                this.Save();
            }
        }
    }

    public void MoveGroupDown(Guid id) {
        lock (this.lockObj) {
            this.EnsureLoaded();
            var index = this.groups.FindIndex(g => g.Id == id);
            if (index >= 0 && index < this.groups.Count - 1) {
                var group = this.groups[index];
                this.groups.RemoveAt(index);
                this.groups.Insert(index + 1, group);
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
            this.groups = new List<CharacterGroup>();
            this.loadedCharacterId = string.Empty;
        }
        this.CacheCleared?.Invoke();
    }
}