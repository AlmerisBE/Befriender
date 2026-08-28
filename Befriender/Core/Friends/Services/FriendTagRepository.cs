namespace Befriender.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class FriendTagRepository : IFriendTagRepository {
    private List<FriendTag> tags = new();
    private readonly object lockObj = new();
    private IFriendTagStorage storage;
    private ICharacterIdentityService identityService;
    private string loadedCharacterId = string.Empty;

    public event Action? CacheCleared;

    public FriendTagRepository(IFriendTagStorage storage, ICharacterIdentityService identityService) {
        this.storage = storage;
        this.identityService = identityService;
    }

    private void EnsureLoaded() {
        var currentId = this.identityService.GetCurrentCharacterId();
        if (!string.IsNullOrEmpty(currentId) && this.loadedCharacterId != currentId) {
            this.tags = this.storage.Load(currentId).ToList();
            this.loadedCharacterId = currentId;
        }
    }

    private string SanitizeTagName(string name) {
        if (string.IsNullOrWhiteSpace(name)) {
            return string.Empty;
        }

        var trimmed = name.Trim();
        return trimmed.Length > 30 ? trimmed[..30] : trimmed;
    }

    public IReadOnlyList<FriendTag> GetTags() {
        lock (this.lockObj) {
            this.EnsureLoaded();
            return this.tags.ToList();
        }
    }

    public void AddTag(string name) {
        var sanitized = this.SanitizeTagName(name);
        if (string.IsNullOrEmpty(sanitized)) {
            return;
        }

        lock (this.lockObj) {
            this.EnsureLoaded();

            if (this.tags.Any(t => t.Name.Equals(sanitized, StringComparison.OrdinalIgnoreCase))) {
                return;
            }

            this.tags.Add(new FriendTag { Name = sanitized });
            this.Save();
        }
    }

    public void UpdateTag(FriendTag tag) {
        var sanitized = this.SanitizeTagName(tag.Name);
        if (string.IsNullOrEmpty(sanitized)) {
            return;
        }

        lock (this.lockObj) {
            this.EnsureLoaded();
            var existing = this.tags.FirstOrDefault(t => t.Id == tag.Id);
            if (existing != null) {
                // Check if another tag already uses this name
                if (this.tags.Any(t => t.Id != tag.Id && t.Name.Equals(sanitized, StringComparison.OrdinalIgnoreCase))) {
                    return;
                }

                existing.Name = sanitized;
                this.Save();
            }
        }
    }

    public void RemoveTag(Guid id) {
        lock (this.lockObj) {
            this.EnsureLoaded();
            this.tags.RemoveAll(t => t.Id == id);
            this.Save();
        }
    }

    public void Save() {
        lock (this.lockObj) {
            this.storage.Save(this.loadedCharacterId, this.tags);
        }
    }

    public void ClearCache() {
        lock (this.lockObj) {
            this.tags = new List<FriendTag>();
            this.loadedCharacterId = string.Empty;
        }
        this.CacheCleared?.Invoke();
    }
}