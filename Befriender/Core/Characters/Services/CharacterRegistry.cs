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
            var previousOnlineStates = this.masterList.ToDictionary(c => c.Id, c => c.IsOnline);
            var sourceState = source.GetCurrentState().ToList();
            var processedIds = new HashSet<Guid>();

            // 1. Merge incoming data and persist isolated source state
            foreach (var incoming in sourceState) {
                var existing = this.masterList.FirstOrDefault(c => c.IsSameIdentity(incoming.ContentId, incoming.Name, incoming.HomeWorldId));

                if (existing == null) {
                    existing = incoming;
                    if (existing.Id == Guid.Empty) existing.Id = Guid.NewGuid();
                    if (existing.AddedAt == DateTime.MinValue) existing.AddedAt = DateTime.Now;

                    if (!existing.SourceSpecificData.ContainsKey(source.SourceId)) {
                        existing.SourceSpecificData[source.SourceId] = new Dictionary<string, string>();
                    }

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

                    if (!existing.SourceSpecificData.ContainsKey(source.SourceId)) {
                        existing.SourceSpecificData[source.SourceId] = new Dictionary<string, string>();
                    }
                }

                if (incoming.SourceSpecificData.TryGetValue(source.SourceId, out var incomingSpecific)) {
                    foreach (var kvp in incomingSpecific) {
                        existing.SourceSpecificData[source.SourceId][kvp.Key] = kvp.Value;
                    }
                }

                existing.SourceSpecificData[source.SourceId]["IsOnline"] = incoming.IsOnline.ToString();
                existing.SourceSpecificData[source.SourceId]["OnlineStateMask"] = incoming.OnlineStateMask.ToString();

                if (incoming.IsOnline) {
                    existing.LastSeenAt = DateTime.Now;
                    if (incoming.CurrentWorldId > 0) existing.CurrentWorldId = incoming.CurrentWorldId;
                    if (incoming.LocationId > 0) existing.LocationId = incoming.LocationId;
                    if (incoming.JobId > 0) existing.JobId = incoming.JobId;
                    if (incoming.Level > 0) existing.Level = incoming.Level;
                }

                if (!string.IsNullOrEmpty(incoming.FcTag)) existing.FcTag = incoming.FcTag;

                existing.ActiveSourceIds.Add(source.SourceId);
                processedIds.Add(existing.Id);
            }

            // 2. Remove isolated state for characters no longer present in this source
            foreach (var character in this.masterList) {
                if (character.ActiveSourceIds.Contains(source.SourceId) && !processedIds.Contains(character.Id)) {
                    character.ActiveSourceIds.Remove(source.SourceId);
                    character.SourceSpecificData.Remove(source.SourceId);
                    if (!character.IsActivelyTracked) {
                        character.IsOnline = false;
                    }
                }
            }

            // 3. Re-evaluate absolute online state across ALL sources
            foreach (var character in this.masterList) {
                if (!character.IsActivelyTracked) continue;

                bool isAnySourceOnline = false;
                ulong activeMask = 0;

                foreach (var sourceId in character.ActiveSourceIds) {
                    if (character.SourceSpecificData.TryGetValue(sourceId, out var data)) {
                        if (data.TryGetValue("IsOnline", out var isOnlineStr) && bool.TryParse(isOnlineStr, out var isOnline) && isOnline) {
                            isAnySourceOnline = true;
                            if (data.TryGetValue("OnlineStateMask", out var maskStr) && ulong.TryParse(maskStr, out var mask)) {
                                activeMask = mask;
                            }
                        }
                    }
                }

                character.IsOnline = isAnySourceOnline;
                if (isAnySourceOnline) character.OnlineStateMask = activeMask;
            }

            if (!string.IsNullOrEmpty(this.currentAccountIdentity)) {
                var localChar = this.masterList.FirstOrDefault(c => $"{c.Name}_{c.HomeWorldId}" == this.currentAccountIdentity);
                if (localChar != null && !localChar.IsOnline) {
                    localChar.IsOnline = true;
                    localChar.LastSeenAt = DateTime.Now;
                }
            }

            this.SaveMasterList();

            // 4. Diff states for notifications
            foreach (var character in this.masterList) {
                bool wasOnline = previousOnlineStates.TryGetValue(character.Id, out var online) && online;

                if (!wasOnline && character.IsOnline) {
                    newlyOnlineCharacters.Add(character);
                }
            }
        }

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

    public void RequestRefresh(bool isUserInitiated = false) {
        lock (this.lockObj) {
            foreach (var source in this.sources) {
                source.RequestRefresh(isUserInitiated);
            }
        }
    }

    public void RequestRefresh(IEnumerable<Guid> sourceIds, bool isUserInitiated = false) {
        lock (this.lockObj) {
            var targetSources = this.sources.Where(s => sourceIds.Contains(s.SourceId));
            foreach (var source in targetSources) {
                source.RequestRefresh(isUserInitiated);
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