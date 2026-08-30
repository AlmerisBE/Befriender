namespace Befriender.Core.FreeCompany.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.FreeCompany.Models;
using Befriender.Core.Friends.Contracts; // Used for ICharacterIdentityService, consider moving this to a shared Core.Identity module later
using System;
using System.Collections.Generic;
using System.Linq;

public class FreeCompanyRepository : ICharacterSource {
    private List<Character> cachedMembers = new();
    private readonly object lockObj = new();
    private ICharacterStorage storage;
    private ICharacterIdentityService identityService;
    private string loadedCharacterId = string.Empty;

    public Guid SourceId { get; } = Guid.Parse("B2C3D4E5-F6A7-4B8C-9D0E-1A2B3C4D5E6F");
    public string Name => "FreeCompany";
    public int Priority => 15; // Higher priority than FriendList (10), but lower than Proximity/Party
    public bool IsEnabled { get; set; } = true;
    public event Action? DataUpdated;

    public FreeCompanyRepository(ICharacterStorage storage, ICharacterIdentityService identityService) {
        this.storage = storage;
        this.identityService = identityService;
    }

    private void EnsureLoaded() {
        var currentId = this.identityService.GetCurrentCharacterId();
        if (!string.IsNullOrEmpty(currentId) && this.loadedCharacterId != currentId) {
            this.cachedMembers = this.storage.Load("FreeCompanyList", currentId).ToList();
            this.loadedCharacterId = currentId;
        }
    }

    public IEnumerable<Character> GetCharacters() {
        lock (this.lockObj) {
            this.EnsureLoaded();
            return this.cachedMembers.ToList();
        }
    }

    public void UpdateMembers(IEnumerable<FreeCompanyMemberProfile> scannedMembers) {
        lock (this.lockObj) {
            var currentId = this.identityService.GetCurrentCharacterId();
            if (string.IsNullOrEmpty(currentId)) {
                return;
            }

            this.EnsureLoaded();

            var repositoryDict = this.cachedMembers.ToDictionary(c => c.ContentId);
            var now = DateTime.Now;

            foreach (var scanned in scannedMembers) {
                if (repositoryDict.TryGetValue(scanned.ContentId, out var existing)) {
                    existing.Name = scanned.Name;
                    existing.HomeWorldId = scanned.HomeWorldId;
                    existing.CurrentWorldId = scanned.CurrentWorldId;
                    existing.IsOnline = scanned.IsOnline;

                    if (scanned.IsOnline) {
                        existing.LocationId = scanned.LocationId;
                        existing.LastSeenAt = now;
                    }

                    if (scanned.JobId > 0) {
                        existing.JobId = scanned.JobId;
                    }

                    if (!string.IsNullOrEmpty(scanned.FcTag)) {
                        existing.FcTag = scanned.FcTag;
                    }
                }
                else {
                    var newMember = new Character {
                        Id = Guid.NewGuid(),
                        ContentId = scanned.ContentId,
                        Name = scanned.Name,
                        HomeWorldId = scanned.HomeWorldId,
                        CurrentWorldId = scanned.CurrentWorldId,
                        JobId = scanned.JobId,
                        LocationId = scanned.LocationId,
                        IsOnline = scanned.IsOnline,
                        FcTag = scanned.FcTag,
                        AddedAt = now,
                        LastSeenAt = scanned.IsOnline ? now : DateTime.MinValue
                    };

                    newMember.ActiveSourceIds.Add(this.SourceId);
                    repositoryDict[scanned.ContentId] = newMember;
                }
            }

            // Mark missing members as no longer in FC by removing them from the cache
            // A genuine FC member sync replaces the entire list
            var scannedIds = scannedMembers.Select(m => m.ContentId).ToHashSet();
            var idsToRemove = repositoryDict.Keys.Where(id => !scannedIds.Contains(id)).ToList();

            foreach (var id in idsToRemove) {
                repositoryDict.Remove(id);
            }

            this.cachedMembers = repositoryDict.Values.ToList();
            this.storage.Save("FreeCompanyList", this.loadedCharacterId, this.cachedMembers);
        }

        this.DataUpdated?.Invoke();
    }
}