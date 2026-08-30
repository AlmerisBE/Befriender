namespace Befriender.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterRegistry : ICharacterRegistry {
    private List<ICharacterSource> sources = new();
    private List<Character> consolidatedCache = new();
    private readonly object lockObj = new();

    public event Action? RegistryUpdated;

    public CharacterRegistry(IEnumerable<ICharacterSource> initialSources) {
        foreach (var source in initialSources) {
            this.RegisterSource(source);
        }
    }

    public void RegisterSource(ICharacterSource source) {
        lock (this.lockObj) {
            if (this.sources.Any(s => s.SourceId == source.SourceId)) {
                return;
            }

            this.sources.Add(source);
            source.DataUpdated += this.ConsolidateData;
        }
        this.ConsolidateData();
    }

    public void UnregisterSource(Guid sourceId) {
        lock (this.lockObj) {
            var source = this.sources.FirstOrDefault(s => s.SourceId == sourceId);
            if (source == null) {
                return;
            }

            source.DataUpdated -= this.ConsolidateData;
            this.sources.Remove(source);
        }
        this.ConsolidateData();
    }

    public IReadOnlyList<Character> GetConsolidatedCharacters() {
        lock (this.lockObj) {
            return this.consolidatedCache.ToList();
        }
    }

    public Character? GetCharacterById(Guid id) {
        lock (this.lockObj) {
            return this.consolidatedCache.FirstOrDefault(c => c.Id == id);
        }
    }

    private void ConsolidateData() {
        lock (this.lockObj) {
            var activeSources = this.sources
                .Where(s => s.IsEnabled)
                .OrderBy(s => s.Priority)
                .ToList();

            var newCache = new List<Character>();

            foreach (var source in activeSources) {
                var characters = source.GetCharacters();

                foreach (var sourceChar in characters) {
                    var existing = newCache.FirstOrDefault(c => c.IsSameIdentity(sourceChar.ContentId, sourceChar.Name, sourceChar.HomeWorldId));

                    if (existing == null) {
                        existing = new Character {
                            Id = sourceChar.Id != Guid.Empty ? sourceChar.Id : Guid.NewGuid(),
                            ContentId = sourceChar.ContentId,
                            Name = sourceChar.Name,
                            HomeWorldId = sourceChar.HomeWorldId
                        };
                        newCache.Add(existing);
                    }

                    // Overwrite basic data based on priority order
                    if (sourceChar.ContentId > 0) {
                        existing.ContentId = sourceChar.ContentId;
                    }

                    if (sourceChar.CurrentWorldId > 0) {
                        existing.CurrentWorldId = sourceChar.CurrentWorldId;
                    }

                    if (sourceChar.JobId > 0) {
                        existing.JobId = sourceChar.JobId;
                    }

                    if (sourceChar.Level > 0) {
                        existing.Level = sourceChar.Level;
                    }

                    if (sourceChar.LocationId > 0) {
                        existing.LocationId = sourceChar.LocationId;
                    }

                    if (!string.IsNullOrEmpty(sourceChar.FcTag)) {
                        existing.FcTag = sourceChar.FcTag;
                    }

                    if (sourceChar.OnlineStateMask > 0) {
                        existing.OnlineStateMask = sourceChar.OnlineStateMask;
                    }

                    if (sourceChar.OnlineStatusId > 0) {
                        existing.OnlineStatusId = sourceChar.OnlineStatusId;
                    }

                    if (sourceChar.ClientLanguages > 0) {
                        existing.ClientLanguages = sourceChar.ClientLanguages;
                    }

                    if (sourceChar.TitleId > 0) {
                        existing.TitleId = sourceChar.TitleId;
                    }

                    if (sourceChar.Race > 0) {
                        existing.Race = sourceChar.Race;
                    }

                    if (sourceChar.Tribe > 0) {
                        existing.Tribe = sourceChar.Tribe;
                    }

                    if (sourceChar.Gender > 0) {
                        existing.Gender = sourceChar.Gender;
                    }

                    if (sourceChar.GrandCompany > 0) {
                        existing.GrandCompany = sourceChar.GrandCompany;
                    }

                    if (sourceChar.AddedLocationId > 0) {
                        existing.AddedLocationId = sourceChar.AddedLocationId;
                    }

                    if (sourceChar.AddedAt > DateTime.MinValue) {
                        existing.AddedAt = sourceChar.AddedAt;
                    }

                    if (sourceChar.ArchivedAt > DateTime.MinValue) {
                        existing.ArchivedAt = sourceChar.ArchivedAt;
                    }

                    if (sourceChar.LastSeenAt > existing.LastSeenAt) {
                        existing.LastSeenAt = sourceChar.LastSeenAt;
                    }

                    if (sourceChar.CustomGroupId.HasValue) {
                        existing.CustomGroupId = sourceChar.CustomGroupId;
                    }

                    if (!string.IsNullOrEmpty(sourceChar.Notes)) {
                        existing.Notes = sourceChar.Notes;
                    }

                    // Boolean flags logic: Combine via OR so active states persist across consolidation
                    existing.IsOnline |= sourceChar.IsOnline;
                    existing.IsFantasiaDetected |= sourceChar.IsFantasiaDetected;
                    existing.IsArchived |= sourceChar.IsArchived;
                    existing.IsCharacterDeleted |= sourceChar.IsCharacterDeleted;
                    existing.IsMarkedForRemoval |= sourceChar.IsMarkedForRemoval;
                    existing.IsMissing |= sourceChar.IsMissing;
                    existing.IsTrackedForNotifications |= sourceChar.IsTrackedForNotifications;

                    // Merge collection data
                    foreach (var tag in sourceChar.Tags) {
                        if (!existing.Tags.Contains(tag)) {
                            existing.Tags.Add(tag);
                        }
                    }

                    foreach (var prevName in sourceChar.PreviousNames) {
                        if (!existing.PreviousNames.Contains(prevName)) {
                            existing.PreviousNames.Add(prevName);
                        }
                    }

                    // Merge IPC custom properties
                    foreach (var kvp in sourceChar.CustomProperties) {
                        existing.CustomProperties[kvp.Key] = kvp.Value;
                    }

                    existing.ActiveSourceIds.Add(source.SourceId);
                }
            }

            this.consolidatedCache = newCache;
        }

        this.RegistryUpdated?.Invoke();
    }
}