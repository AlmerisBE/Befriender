namespace Befriender.Core.Friends.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Migrations.Contracts;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class FriendRepository : IFriendRepository, ICharacterSource {
    private List<FriendProfile> friends = new();
    private readonly object lockObj = new();
    private ICharacterStorage storage;
    private IMigrationService migrationService;
    private ICharacterIdentityService identityService;
    private IClientState clientState;
    private IObjectTable objectTable;
    private string loadedCharacterId = string.Empty;

    public event Action? CacheCleared;
    public event Action<FriendProfile>? FriendLoggedOn;

    public Guid SourceId { get; } = Guid.Parse("A1B2C3D4-E5F6-4A7B-8C9D-E0F1A2B3C4D5");
    public string Name => "FriendList";
    public int Priority => 10;
    public bool IsEnabled { get; set; } = true;
    public event Action? DataUpdated;

    public FriendRepository(ICharacterStorage storage, IMigrationService migrationService, ICharacterIdentityService identityService, IClientState clientState, IObjectTable objectTable) {
        this.storage = storage;
        this.migrationService = migrationService;
        this.identityService = identityService;
        this.clientState = clientState;
        this.objectTable = objectTable;
    }

    private void EnsureLoaded() {
        var currentId = this.identityService.GetCurrentCharacterId();
        if (!string.IsNullOrEmpty(currentId) && this.loadedCharacterId != currentId) {
            this.migrationService.RunMigrations(currentId);

            var characters = this.storage.Load("FriendList", currentId);
            this.friends = characters.Select(this.MapFromCharacter).ToList();
            this.loadedCharacterId = currentId;
        }
    }

    private FriendProfile MapFromCharacter(Character character) {
        var profile = new FriendProfile {
            Id = character.Id,
            ContentId = character.ContentId,
            Name = character.Name,
            HomeWorldId = character.HomeWorldId,
            CurrentWorldId = character.CurrentWorldId,
            JobId = character.JobId,
            Level = character.Level,
            LocationId = character.LocationId,
            IsOnline = character.IsOnline,
            FcTag = character.FcTag
        };

        if (character.CustomProperties.TryGetValue("Befriender_OnlineStateMask", out var val) && ulong.TryParse(val, out var mask)) {
            profile.OnlineStateMask = mask;
        }

        if (character.CustomProperties.TryGetValue("Befriender_OnlineStatusId", out val) && byte.TryParse(val, out var statusId)) {
            profile.OnlineStatusId = statusId;
        }

        if (character.CustomProperties.TryGetValue("Befriender_ClientLanguages", out val) && byte.TryParse(val, out var lang)) {
            profile.ClientLanguages = lang;
        }

        if (character.CustomProperties.TryGetValue("Befriender_TitleId", out val) && ushort.TryParse(val, out var titleId)) {
            profile.TitleId = titleId;
        }

        if (character.CustomProperties.TryGetValue("Befriender_Race", out val) && byte.TryParse(val, out var race)) {
            profile.Race = race;
        }

        if (character.CustomProperties.TryGetValue("Befriender_Tribe", out val) && byte.TryParse(val, out var tribe)) {
            profile.Tribe = tribe;
        }

        if (character.CustomProperties.TryGetValue("Befriender_Gender", out val) && byte.TryParse(val, out var gender)) {
            profile.Gender = gender;
        }

        if (character.CustomProperties.TryGetValue("Befriender_IsFantasiaDetected", out val) && bool.TryParse(val, out var fantasia)) {
            profile.IsFantasiaDetected = fantasia;
        }

        if (character.CustomProperties.TryGetValue("Befriender_AddedAt", out val) && DateTime.TryParse(val, null, System.Globalization.DateTimeStyles.RoundtripKind, out var addedAt)) {
            profile.AddedAt = addedAt;
        }

        if (character.CustomProperties.TryGetValue("Befriender_AddedLocationId", out val) && uint.TryParse(val, out var addedLoc)) {
            profile.AddedLocationId = addedLoc;
        }

        if (character.CustomProperties.TryGetValue("Befriender_LastSeenAt", out val) && DateTime.TryParse(val, null, System.Globalization.DateTimeStyles.RoundtripKind, out var lastSeenAt)) {
            profile.LastSeenAt = lastSeenAt;
        }

        if (character.CustomProperties.TryGetValue("Befriender_ArchivedAt", out val) && DateTime.TryParse(val, null, System.Globalization.DateTimeStyles.RoundtripKind, out var archivedAt)) {
            profile.ArchivedAt = archivedAt;
        }

        if (character.CustomProperties.TryGetValue("Befriender_CustomGroupId", out val) && Guid.TryParse(val, out var groupId)) {
            profile.CustomGroupId = groupId;
        }

        if (character.CustomProperties.TryGetValue("Befriender_Tags", out val)) {
            try { profile.Tags = System.Text.Json.JsonSerializer.Deserialize<List<Guid>>(val) ?? new(); } catch { }
        }
        if (character.CustomProperties.TryGetValue("Befriender_PreviousNames", out val)) {
            try { profile.PreviousNames = System.Text.Json.JsonSerializer.Deserialize<List<string>>(val) ?? new(); } catch { }
        }

        if (character.CustomProperties.TryGetValue("Befriender_Notes", out val)) {
            profile.Notes = val;
        }

        if (character.CustomProperties.TryGetValue("Befriender_IsArchived", out val) && bool.TryParse(val, out var isArchived)) {
            profile.IsArchived = isArchived;
        }

        if (character.CustomProperties.TryGetValue("Befriender_IsCharacterDeleted", out val) && bool.TryParse(val, out var isDeleted)) {
            profile.IsCharacterDeleted = isDeleted;
        }

        if (character.CustomProperties.TryGetValue("Befriender_IsMarkedForRemoval", out val) && bool.TryParse(val, out var isMarked)) {
            profile.IsMarkedForRemoval = isMarked;
        }

        if (character.CustomProperties.TryGetValue("Befriender_IsMissing", out val) && bool.TryParse(val, out var isMissing)) {
            profile.IsMissing = isMissing;
        }

        if (character.CustomProperties.TryGetValue("Befriender_GrandCompany", out val) && byte.TryParse(val, out var gc)) {
            profile.GrandCompany = gc;
        }

        if (character.CustomProperties.TryGetValue("Befriender_IsTrackedForNotifications", out val) && bool.TryParse(val, out var isTracked)) {
            profile.IsTrackedForNotifications = isTracked;
        }

        return profile;
    }

    private Character MapToCharacter(FriendProfile profile) {
        var character = new Character {
            Id = profile.Id,
            ContentId = profile.ContentId,
            Name = profile.Name,
            HomeWorldId = profile.HomeWorldId,
            CurrentWorldId = profile.CurrentWorldId,
            JobId = profile.JobId,
            Level = profile.Level,
            LocationId = profile.LocationId,
            IsOnline = profile.IsOnline,
            FcTag = profile.FcTag
        };

        character.ActiveSourceIds.Add(this.SourceId);

        character.CustomProperties["Befriender_OnlineStateMask"] = profile.OnlineStateMask.ToString();
        character.CustomProperties["Befriender_OnlineStatusId"] = profile.OnlineStatusId.ToString();
        character.CustomProperties["Befriender_ClientLanguages"] = profile.ClientLanguages.ToString();
        character.CustomProperties["Befriender_TitleId"] = profile.TitleId.ToString();
        character.CustomProperties["Befriender_Race"] = profile.Race.ToString();
        character.CustomProperties["Befriender_Tribe"] = profile.Tribe.ToString();
        character.CustomProperties["Befriender_Gender"] = profile.Gender.ToString();
        character.CustomProperties["Befriender_IsFantasiaDetected"] = profile.IsFantasiaDetected.ToString();
        character.CustomProperties["Befriender_AddedAt"] = profile.AddedAt.ToString("O");
        character.CustomProperties["Befriender_AddedLocationId"] = profile.AddedLocationId.ToString();
        character.CustomProperties["Befriender_LastSeenAt"] = profile.LastSeenAt.ToString("O");
        character.CustomProperties["Befriender_ArchivedAt"] = profile.ArchivedAt.ToString("O");
        character.CustomProperties["Befriender_CustomGroupId"] = profile.CustomGroupId?.ToString() ?? string.Empty;
        character.CustomProperties["Befriender_Tags"] = System.Text.Json.JsonSerializer.Serialize(profile.Tags);
        character.CustomProperties["Befriender_PreviousNames"] = System.Text.Json.JsonSerializer.Serialize(profile.PreviousNames);
        character.CustomProperties["Befriender_Notes"] = profile.Notes;
        character.CustomProperties["Befriender_IsArchived"] = profile.IsArchived.ToString();
        character.CustomProperties["Befriender_IsCharacterDeleted"] = profile.IsCharacterDeleted.ToString();
        character.CustomProperties["Befriender_IsMarkedForRemoval"] = profile.IsMarkedForRemoval.ToString();
        character.CustomProperties["Befriender_IsMissing"] = profile.IsMissing.ToString();
        character.CustomProperties["Befriender_GrandCompany"] = profile.GrandCompany.ToString();
        character.CustomProperties["Befriender_IsTrackedForNotifications"] = profile.IsTrackedForNotifications.ToString();

        return character;
    }

    public IEnumerable<Character> GetCharacters() {
        lock (this.lockObj) {
            this.EnsureLoaded();
            return this.friends.Select(this.MapToCharacter).ToList();
        }
    }

    public IReadOnlyList<FriendProfile> GetFriends() {
        lock (this.lockObj) {
            this.EnsureLoaded();
            return this.friends.ToList();
        }
    }

    public void UpdateFriends(IEnumerable<FriendProfile> scannedFriends) {
        lock (this.lockObj) {
            var currentId = this.identityService.GetCurrentCharacterId();
            if (string.IsNullOrEmpty(currentId)) {
                return;
            }

            this.EnsureLoaded();

            var currentTerritory = this.clientState.TerritoryType;
            var now = DateTime.Now;
            var scannedList = scannedFriends.ToList();

            var repositoryDict = this.friends.ToDictionary(f => f.ContentId);

            var visiblePlayers = new Dictionary<(string, uint), Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>();
            for (int i = 0; i < this.objectTable.Length; i++) {
                if (this.objectTable[i] is Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter player) {
                    visiblePlayers[(player.Name.TextValue, player.HomeWorld.RowId)] = player;
                }
            }

            foreach (var scanned in scannedList) {
                if (visiblePlayers.TryGetValue((scanned.Name, scanned.HomeWorldId), out var presentPlayer)) {
                    scanned.IsOnline = true;
                    scanned.JobId = (byte)presentPlayer.ClassJob.RowId;
                }

                if (repositoryDict.TryGetValue(scanned.ContentId, out var existing)) {
                    bool isDeletedChar = string.IsNullOrWhiteSpace(scanned.Name);
                    bool wasOffline = !existing.IsOnline;

                    existing.IsCharacterDeleted = isDeletedChar;

                    if (!isDeletedChar) {
                        if (!string.Equals(existing.Name, scanned.Name, StringComparison.Ordinal) && !string.IsNullOrEmpty(existing.Name)) {
                            existing.PreviousNames ??= new List<string>();
                            if (!existing.PreviousNames.Contains(existing.Name)) {
                                existing.PreviousNames.Add(existing.Name);
                            }
                        }
                        existing.Name = scanned.Name;
                    }

                    existing.IsArchived = false;
                    existing.IsOnline = scanned.IsOnline;
                    existing.HomeWorldId = scanned.HomeWorldId;
                    existing.CurrentWorldId = scanned.CurrentWorldId;
                    existing.ClientLanguages = scanned.ClientLanguages;
                    existing.GrandCompany = scanned.GrandCompany;

                    if (scanned.IsOnline) {
                        existing.LastSeenAt = now;
                        existing.OnlineStateMask = scanned.OnlineStateMask;
                        existing.LocationId = scanned.LocationId;
                    }

                    if (scanned.JobId > 0) {
                        existing.JobId = scanned.JobId;
                    }

                    if (!string.IsNullOrEmpty(scanned.FcTag)) {
                        existing.FcTag = scanned.FcTag;
                    }

                    if (wasOffline && existing.IsOnline) {
                        this.FriendLoggedOn?.Invoke(existing);
                    }
                }
                else {
                    scanned.Id = Guid.NewGuid();
                    scanned.IsCharacterDeleted = string.IsNullOrWhiteSpace(scanned.Name);
                    scanned.AddedAt = now;
                    scanned.AddedLocationId = currentTerritory;
                    scanned.LastSeenAt = scanned.IsOnline ? now : DateTime.MinValue;
                    repositoryDict[scanned.ContentId] = scanned;
                }
            }

            var scannedIds = scannedList.Select(f => f.ContentId).ToHashSet();
            foreach (var existing in repositoryDict.Values) {
                if (!scannedIds.Contains(existing.ContentId)) {
                    existing.IsArchived = true;
                    existing.ArchivedAt = now;
                    existing.IsOnline = false;
                    existing.IsMarkedForRemoval = false;

                    if (visiblePlayers.TryGetValue((existing.Name, existing.HomeWorldId), out var presentPlayer)) {
                        existing.IsOnline = true;
                        existing.JobId = (byte)presentPlayer.ClassJob.RowId;
                        existing.LastSeenAt = now;
                    }
                }
            }

            this.friends = repositoryDict.Values.ToList();
            var charactersToSave = this.friends.Select(this.MapToCharacter).ToList();
            this.storage.Save("FriendList", this.loadedCharacterId, charactersToSave);
        }

        this.DataUpdated?.Invoke();
    }

    public void UpdateFriendFromCharacter(ulong contentId, IPlayerCharacter player, uint territoryId) {
        lock (this.lockObj) {
            this.EnsureLoaded();
            var friend = this.friends.FirstOrDefault(f => f.ContentId == contentId);
            if (friend == null) {
                return;
            }

            bool changed = false;

            if (friend.Level != player.Level) { friend.Level = player.Level; changed = true; }
            if (friend.JobId != player.ClassJob.RowId) { friend.JobId = (byte)player.ClassJob.RowId; changed = true; }

            var tag = player.CompanyTag.TextValue;
            if (friend.FcTag != tag) { friend.FcTag = tag; changed = true; }

            var localPlayer = this.objectTable.LocalPlayer;
            if (localPlayer != null && friend.CurrentWorldId != localPlayer.CurrentWorld.RowId) {
                friend.CurrentWorldId = localPlayer.CurrentWorld.RowId;
                changed = true;
            }

            unsafe {
                var csChar = (FFXIVClientStructs.FFXIV.Client.Game.Character.Character*)player.Address;
                if (csChar != null) {
                    if (friend.TitleId != csChar->TitleId) { friend.TitleId = csChar->TitleId; changed = true; }
                    if (friend.OnlineStatusId != csChar->CharacterData.OnlineStatus) { friend.OnlineStatusId = csChar->CharacterData.OnlineStatus; changed = true; }

                    byte race = csChar->DrawData.CustomizeData.Race;
                    byte tribe = csChar->DrawData.CustomizeData.Tribe;
                    byte gender = csChar->DrawData.CustomizeData.Sex;

                    if (friend.Race != 0 && (friend.Race != race || friend.Gender != gender)) {
                        friend.IsFantasiaDetected = true;
                        changed = true;
                    }

                    if (friend.Race != race) { friend.Race = race; changed = true; }
                    if (friend.Tribe != tribe) { friend.Tribe = tribe; changed = true; }
                    if (friend.Gender != gender) { friend.Gender = gender; changed = true; }
                }
            }

            if (friend.LocationId != territoryId) { friend.LocationId = territoryId; changed = true; }

            if (!friend.IsOnline) { friend.IsOnline = true; changed = true; }
            if (friend.IsMissing) { friend.IsMissing = false; changed = true; }

            if ((DateTime.Now - friend.LastSeenAt).TotalMinutes > 5) {
                friend.LastSeenAt = DateTime.Now;
                changed = true;
            }

            if (changed) {
                this.Save();
            }
        }
    }

    public void RemoveFriendData(ulong contentId) {
        lock (this.lockObj) {
            this.friends.RemoveAll(f => f.ContentId == contentId);
            this.Save();
        }
    }

    public void Save() {
        lock (this.lockObj) {
            var charactersToSave = this.friends.Select(this.MapToCharacter).ToList();
            this.storage.Save("FriendList", this.loadedCharacterId, charactersToSave);
        }
        this.DataUpdated?.Invoke();
    }

    public void ClearCache() {
        lock (this.lockObj) {
            this.friends = new List<FriendProfile>();
            this.loadedCharacterId = string.Empty;
        }

        this.CacheCleared?.Invoke();
        this.DataUpdated?.Invoke();
    }
}