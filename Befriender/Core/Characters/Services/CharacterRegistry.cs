namespace Befriender.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterRegistry : ICharacterRegistry {
    private List<ICharacterSource> sources = new();
    private List<Character> masterList = new();
    private ICharacterStorage storage;
    private string currentAccountIdentity = string.Empty;
    private readonly object lockObj = new();

    public event Action? RegistryUpdated;

    public CharacterRegistry(ICharacterStorage storage, IEnumerable<ICharacterSource> initialSources) {
        this.storage = storage;
        foreach (var source in initialSources) {
            this.RegisterSource(source);
        }
    }

    public void LoadMasterList(string accountIdentity) {
        lock (this.lockObj) {
            this.currentAccountIdentity = accountIdentity;
            this.masterList = this.storage.Load("MasterCharacterList", accountIdentity).ToList();
        }
        this.RegistryUpdated?.Invoke();
    }

    public void SaveMasterList() {
        lock (this.lockObj) {
            if (!string.IsNullOrEmpty(this.currentAccountIdentity)) {
                this.storage.Save("MasterCharacterList", this.currentAccountIdentity, this.masterList);
            }
        }
    }

    public void RegisterSource(ICharacterSource source) {
        lock (this.lockObj) {
            if (this.sources.Any(s => s.SourceId == source.SourceId)) {
                return;
            }

            this.sources.Add(source);
            source.DataUpdated += () => this.ProcessSourceUpdate(source);
        }
    }

    public void UnregisterSource(Guid sourceId) {
        lock (this.lockObj) {
            var source = this.sources.FirstOrDefault(s => s.SourceId == sourceId);
            if (source == null) {
                return;
            }

            this.sources.Remove(source);
            // We do NOT remove the SourceId from characters here. Unregistering a source
            // just means the module is disabled, not that the characters left the source.
        }
    }

    private void ProcessSourceUpdate(ICharacterSource source) {
        lock (this.lockObj) {
            var sourceState = source.GetCurrentState().ToList();
            var sourceIdsInUpdate = sourceState.Select(c => c.ContentId).ToHashSet();

            foreach (var incoming in sourceState) {
                var existing = this.masterList.FirstOrDefault(c => c.IsSameIdentity(incoming.ContentId, incoming.Name, incoming.HomeWorldId));

                if (existing == null) {
                    existing = incoming;
                    if (existing.Id == Guid.Empty) {
                        existing.Id = Guid.NewGuid();
                    }

                    if (existing.AddedAt == DateTime.MinValue) {
                        existing.AddedAt = DateTime.Now;
                    }

                    this.masterList.Add(existing);
                }
                else {
                    // Update Intrinsic Data
                    if (incoming.ContentId > 0) {
                        existing.ContentId = incoming.ContentId;
                    }

                    if (incoming.Race > 0) {
                        existing.Race = incoming.Race;
                    }

                    if (incoming.Tribe > 0) {
                        existing.Tribe = incoming.Tribe;
                    }

                    if (incoming.Gender > 0) {
                        existing.Gender = incoming.Gender;
                    }

                    if (!string.Equals(existing.Name, incoming.Name, StringComparison.Ordinal) && !string.IsNullOrEmpty(existing.Name)) {
                        if (!existing.PreviousNames.Contains(existing.Name)) {
                            existing.PreviousNames.Add(existing.Name);
                        }
                        existing.Name = incoming.Name;
                    }

                    // Update Volatile Data (Later we can implement Priority checks here)
                    existing.IsOnline = incoming.IsOnline;
                    existing.CurrentWorldId = incoming.CurrentWorldId;
                    existing.LocationId = incoming.LocationId;

                    if (incoming.IsOnline) {
                        existing.LastSeenAt = DateTime.Now;
                        existing.OnlineStateMask = incoming.OnlineStateMask;
                    }

                    if (incoming.JobId > 0) {
                        existing.JobId = incoming.JobId;
                    }

                    if (incoming.Level > 0) {
                        existing.Level = incoming.Level;
                    }

                    if (!string.IsNullOrEmpty(incoming.FcTag)) {
                        existing.FcTag = incoming.FcTag;
                    }

                    // Update Source Specific Data
                    if (incoming.SourceSpecificData.TryGetValue(source.SourceId, out var specificData)) {
                        existing.SourceSpecificData[source.SourceId] = specificData;
                    }
                }

                existing.ActiveSourceIds.Add(source.SourceId);
            }

            // Remove this source's ID from characters that are no longer present in the source update
            foreach (var character in this.masterList) {
                if (character.ActiveSourceIds.Contains(source.SourceId) && !sourceIdsInUpdate.Contains(character.ContentId)) {
                    character.ActiveSourceIds.Remove(source.SourceId);

                    // If the character drops offline because it left the source, update it
                    if (!character.IsActivelyTracked) {
                        character.IsOnline = false;
                    }
                }
            }

            this.SaveMasterList();
        }

        this.RegistryUpdated?.Invoke();
    }

    public IReadOnlyList<Character> GetAllCharacters() {
        lock (this.lockObj) {
            return this.masterList.ToList();
        }
    }

    public IReadOnlyList<Character> GetCharactersBySource(Guid sourceId) {
        lock (this.lockObj) {
            return this.masterList.Where(c => c.ActiveSourceIds.Contains(sourceId)).ToList();
        }
    }

    public Character? GetCharacterById(Guid id) {
        lock (this.lockObj) {
            return this.masterList.FirstOrDefault(c => c.Id == id);
        }
    }
}