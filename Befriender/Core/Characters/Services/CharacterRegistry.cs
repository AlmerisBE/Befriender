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

    public void RegisterSource(ICharacterSource source) {
        lock (this.lockObj) {
            if (this.sources.Any(s => s.SourceId.Equals(source.SourceId, StringComparison.OrdinalIgnoreCase))) {
                return;
            }

            this.sources.Add(source);
            source.DataUpdated += this.ConsolidateData;
        }

        this.ConsolidateData();
    }

    public void UnregisterSource(string sourceId) {
        lock (this.lockObj) {
            var source = this.sources.FirstOrDefault(s => s.SourceId.Equals(sourceId, StringComparison.OrdinalIgnoreCase));
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
                            ContentId = sourceChar.ContentId,
                            Name = sourceChar.Name,
                            HomeWorldId = sourceChar.HomeWorldId
                        };
                        newCache.Add(existing);
                    }

                    // Lower priority data is naturally overwritten by higher priority data due to the loop order
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

                    // Boolean flags are tricky; we assume higher priority defines the definitive state
                    existing.IsOnline = sourceChar.IsOnline;

                    existing.ActiveSources.Add(source.SourceId);
                }
            }

            this.consolidatedCache = newCache;
        }

        this.RegistryUpdated?.Invoke();
    }
}