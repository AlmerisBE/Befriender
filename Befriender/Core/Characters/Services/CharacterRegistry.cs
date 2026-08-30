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

                    // Always favor online status if any source detects it
                    if (sourceChar.IsOnline) {
                        existing.IsOnline = true;
                    }

                    // Merge custom properties (higher priority overwrites)
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