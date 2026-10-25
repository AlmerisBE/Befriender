namespace Befriender.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Migrations.Contracts;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class CharacterRegistry : ICharacterRegistry, IDisposable {
    public event Action<Character>? CharacterLoggedOn;
    private List<ICharacterSource> sources = new();
    private List<Character> masterList = new();

    private ICharacterStorage storage;
    private IMigrationService migrationService;
    private ICharacterIdentityService identityService;
    private IClientState clientState;
    private IFramework framework;
    private IPluginLog pluginLog;
    private ILocalPlayerContext localPlayerContext;

    private string currentAccountIdentity = string.Empty;
    private readonly object lockObj = new();

    public event Action? RegistryUpdated;

    public CharacterRegistry(
        ICharacterStorage storage,
        IEnumerable<ICharacterSource> initialSources,
        IMigrationService migrationService,
        ICharacterIdentityService identityService,
        IClientState clientState,
        IFramework framework,
        IPluginLog pluginLog,
        ILocalPlayerContext localPlayerContext) {

        this.storage = storage;
        this.migrationService = migrationService;
        this.identityService = identityService;
        this.clientState = clientState;
        this.framework = framework;
        this.pluginLog = pluginLog;
        this.localPlayerContext = localPlayerContext;

        foreach (var source in initialSources) {
            this.RegisterSource(source);
        }

        this.clientState.Logout += this.OnLogout;
        this.framework.Update += this.OnFrameworkUpdate;
    }

    private void OnFrameworkUpdate(IFramework fw) {
        if (this.clientState.IsLoggedIn && string.IsNullOrEmpty(this.currentAccountIdentity)) {
            var accountId = this.identityService.GetCurrentCharacterId();
            if (!string.IsNullOrEmpty(accountId)) {
                this.pluginLog.Debug($"[CharacterRegistry] LocalPlayer detected: {accountId}. Initializing registry...");
                this.migrationService.RunMigrations(accountId);
                this.LoadMasterList(accountId);
            }
        }
    }

    private void OnLogout(int type, int code) {
        lock (this.lockObj) {
            this.masterList.Clear();
            this.currentAccountIdentity = string.Empty;
        }
        this.RegistryUpdated?.Invoke();
    }

    public void LoadMasterList(string accountIdentity) {
        lock (this.lockObj) {
            this.currentAccountIdentity = accountIdentity;
            this.masterList = this.storage.Load("MasterCharacterList", accountIdentity).ToList();

            var localChar = this.masterList.FirstOrDefault(c => $"{c.Name}_{c.HomeWorldId}" == accountIdentity);
            if (localChar != null) {
                localChar.IsOnline = true;
                localChar.LastSeenAt = DateTime.Now;
            }
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
            if (this.sources.Any(s => s.SourceId == source.SourceId)) return;

            this.sources.Add(source);
            source.DataUpdated += () => this.ProcessSourceUpdate(source);
        }
    }

    public void UnregisterSource(Guid sourceId) {
        lock (this.lockObj) {
            var source = this.sources.FirstOrDefault(s => s.SourceId == sourceId);
            if (source == null) return;

            this.sources.Remove(source);
        }
    }

    private void ProcessSourceUpdate(ICharacterSource source) {
        var newlyOnlineCharacters = new List<Character>();

        lock (this.lockObj) {
            // 1. Snapshot the exact online state of all known characters before the rebuild
            var previousOnlineStates = this.masterList.ToDictionary(c => c.Id, c => c.IsOnline);

            var sourceState = source.GetCurrentState().ToList();
            var processedIds = new HashSet<Guid>();

            // 2. Perform the normal source aggregation
            foreach (var incoming in sourceState) {
                var existing = this.masterList.FirstOrDefault(c => c.IsSameIdentity(incoming.ContentId, incoming.Name, incoming.HomeWorldId));

                if (existing == null) {
                    existing = incoming;
                    if (existing.Id == Guid.Empty) existing.Id = Guid.NewGuid();
                    if (existing.AddedAt == DateTime.MinValue) existing.AddedAt = DateTime.Now;

                    this.masterList.Add(existing);
                }
                else {
                    if (incoming.ContentId > 0) existing.ContentId = incoming.ContentId;
                    if (incoming.Race > 0) existing.Race = incoming.Race;
                    if (incoming.Tribe > 0) existing.Tribe = incoming.Tribe;
                    if (incoming.Gender > 0) existing.Gender = incoming.Gender;

                    if (!string.Equals(existing.Name, incoming.Name, StringComparison.Ordinal) && !string.IsNullOrEmpty(existing.Name)) {
                        if (!existing.PreviousNames.Contains(existing.Name)) {
                            existing.PreviousNames.Add(existing.Name);
                        }
                        existing.Name = incoming.Name;
                    }

                    existing.IsOnline = incoming.IsOnline;

                    if (incoming.CurrentWorldId > 0) existing.CurrentWorldId = incoming.CurrentWorldId;
                    if (incoming.LocationId > 0) existing.LocationId = incoming.LocationId;

                    if (incoming.IsOnline) {
                        existing.LastSeenAt = DateTime.Now;
                        existing.OnlineStateMask = incoming.OnlineStateMask;
                    }

                    if (incoming.JobId > 0) existing.JobId = incoming.JobId;
                    if (incoming.Level > 0) existing.Level = incoming.Level;
                    if (!string.IsNullOrEmpty(incoming.FcTag)) existing.FcTag = incoming.FcTag;

                    if (incoming.SourceSpecificData.TryGetValue(source.SourceId, out var specificData)) {
                        existing.SourceSpecificData[source.SourceId] = specificData;
                    }
                }

                existing.ActiveSourceIds.Add(source.SourceId);
                processedIds.Add(existing.Id);
            }

            foreach (var character in this.masterList) {
                if (character.ActiveSourceIds.Contains(source.SourceId) && !processedIds.Contains(character.Id)) {
                    character.ActiveSourceIds.Remove(source.SourceId);
                    if (!character.IsActivelyTracked) {
                        character.IsOnline = false;
                    }
                }
            }

            if (!string.IsNullOrEmpty(this.currentAccountIdentity)) {
                var localChar = this.masterList.FirstOrDefault(c => $"{c.Name}_{c.HomeWorldId}" == this.currentAccountIdentity);
                if (localChar != null && !localChar.IsOnline) {
                    localChar.IsOnline = true;
                    localChar.LastSeenAt = DateTime.Now;
                }
            }

            this.SaveMasterList();

            // 3. Diff the states and queue notifications only on true transitions
            foreach (var character in this.masterList) {
                bool wasOnline = previousOnlineStates.TryGetValue(character.Id, out var online) && online;

                if (!wasOnline && character.IsOnline) {
                    newlyOnlineCharacters.Add(character);
                }
            }
        }

        // Fire events outside the lock to prevent deadlocks with UI threads
        foreach (var character in newlyOnlineCharacters) {
            this.CharacterLoggedOn?.Invoke(character);
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

    public void RemoveCharacter(Guid id) {
        ulong currentLocalId = this.localPlayerContext.GetLocalContentId();
        bool stateChanged = false;

        lock (this.lockObj) {
            var character = this.masterList.FirstOrDefault(c => c.Id == id);
            if (character == null) return;

            if (currentLocalId > 0 && character.AssociatedLocalPlayerIds.Contains(currentLocalId)) {
                character.AssociatedLocalPlayerIds.Remove(currentLocalId);
                stateChanged = true;
            }

            if (character.AssociatedLocalPlayerIds.Count == 0 && !character.IsActivelyTracked) {
                this.masterList.Remove(character);
                stateChanged = true;
            }

            if (stateChanged) this.SaveMasterList();
        }

        if (stateChanged) this.RegistryUpdated?.Invoke();
    }

    public void RequestManualRefresh() {
        lock (this.lockObj) {
            foreach (var source in this.sources) {
                source.RequestManualRefresh();
            }
        }
    }

    public void RequestManualRefresh(IEnumerable<Guid> sourceIds) {
        lock (this.lockObj) {
            var targetSources = this.sources.Where(s => sourceIds.Contains(s.SourceId));
            foreach (var source in targetSources) {
                source.RequestManualRefresh();
            }
        }
    }

    public void NotifyDataChanged() {
        this.RegistryUpdated?.Invoke();
    }

    public bool TryAddManualCharacter(ulong contentId, string name, uint homeWorldId, uint locationId) {
        if (string.IsNullOrWhiteSpace(name)) return false;

        ulong currentLocalPlayerId = this.localPlayerContext.GetLocalContentId();
        if (currentLocalPlayerId == 0) return false;

        lock (this.lockObj) {
            var existing = this.masterList.FirstOrDefault(c => c.IsSameIdentity(contentId, name, homeWorldId));

            if (existing != null) {
                if (!existing.AssociatedLocalPlayerIds.Contains(currentLocalPlayerId)) {
                    existing.AssociatedLocalPlayerIds.Add(currentLocalPlayerId);
                }

                if (contentId > 0) existing.ContentId = contentId;
                if (locationId > 0 && existing.AddedLocationId == 0) existing.AddedLocationId = locationId;

                this.SaveMasterList();
                this.NotifyDataChanged();
                return true;
            }

            var newCharacter = new Character {
                Id = Guid.NewGuid(),
                ContentId = contentId,
                Name = name.Trim(),
                HomeWorldId = homeWorldId,
                AddedAt = DateTime.Now,
                AddedLocationId = locationId,
                LastSeenAt = DateTime.Now,
                IsOnline = true
            };

            newCharacter.AssociatedLocalPlayerIds.Add(currentLocalPlayerId);

            this.masterList.Add(newCharacter);
            this.SaveMasterList();
        }

        this.NotifyDataChanged();
        return true;
    }

    public void Dispose() {
        this.clientState.Logout -= this.OnLogout;
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}