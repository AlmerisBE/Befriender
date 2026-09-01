namespace Befriender.Core.Migrations.Implementations;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Migrations.Contracts;
using Dalamud.Plugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class V1LegacyFriendStorageMigration : IMigration {
    private IDalamudPluginInterface pluginInterface;
    private ICharacterStorage characterStorage;

    public int TargetVersion => 1;

    public V1LegacyFriendStorageMigration(IDalamudPluginInterface pluginInterface, ICharacterStorage characterStorage) {
        this.pluginInterface = pluginInterface;
        this.characterStorage = characterStorage;
    }

    public void Execute(string accountIdentity) {
        string legacyPath = Path.Combine(this.pluginInterface.ConfigDirectory.FullName, $"friends_{accountIdentity}.json");

        if (!File.Exists(legacyPath)) {
            return;
        }

        try {
            string json = File.ReadAllText(legacyPath);

            // Permissive deserialization to capture legacy camelCase properties
            var options = new JsonSerializerOptions {
                PropertyNameCaseInsensitive = true,
                IncludeFields = true
            };

            var legacyProfiles = JsonSerializer.Deserialize<List<LegacyFriendProfile>>(json, options);

            if (legacyProfiles != null && legacyProfiles.Count > 0) {
                var characters = new List<Character>();

                foreach (var profile in legacyProfiles) {
                    var character = new Character {
                        Id = profile.Id != Guid.Empty ? profile.Id : Guid.NewGuid(),
                        ContentId = profile.ContentId,
                        Name = profile.IsCharacterDeleted ? string.Empty : (profile.Name ?? string.Empty),
                        HomeWorldId = profile.HomeWorldId,
                        CurrentWorldId = profile.CurrentWorldId,
                        JobId = profile.JobId,
                        Level = profile.Level,
                        LocationId = profile.LocationId,
                        IsOnline = profile.IsOnline,
                        FcTag = profile.FcTag ?? string.Empty,
                        OnlineStateMask = profile.OnlineStateMask,
                        OnlineStatusId = profile.OnlineStatusId,
                        ClientLanguages = profile.ClientLanguages,
                        TitleId = profile.TitleId,
                        Race = profile.Race,
                        Tribe = profile.Tribe,
                        Gender = profile.Gender,
                        IsFantasiaDetected = profile.IsFantasiaDetected,
                        AddedAt = profile.AddedAt,
                        AddedLocationId = profile.AddedLocationId,
                        LastSeenAt = profile.LastSeenAt,
                        CustomGroupId = profile.CustomGroupId,
                        Tags = profile.Tags ?? new List<Guid>(),
                        PreviousNames = profile.PreviousNames ?? new List<string>(),
                        Notes = profile.Notes ?? string.Empty,
                        GrandCompany = profile.GrandCompany,
                        IsTrackedForNotifications = profile.IsTrackedForNotifications
                    };

                    if (!profile.IsArchived && !profile.IsMissing) {
                        character.ActiveSourceIds.Add(Guid.Parse("51000000-0000-0000-0000-000000000001"));
                    }

                    characters.Add(character);
                }

                this.characterStorage.Save("MasterCharacterList", accountIdentity, characters);
            }

            File.Move(legacyPath, $"{legacyPath}.migrated");
        }
        catch {
            // Failsafe
        }
    }

    private class LegacyFriendProfile {
        public Guid Id { get; set; }
        public ulong ContentId { get; set; }
        public string Name { get; set; } = string.Empty;
        public uint HomeWorldId { get; set; }
        public uint CurrentWorldId { get; set; }
        public bool IsOnline { get; set; }
        public byte JobId { get; set; }
        public byte Level { get; set; }
        public uint LocationId { get; set; }
        public string FcTag { get; set; } = string.Empty;
        public ulong OnlineStateMask { get; set; }
        public byte OnlineStatusId { get; set; }
        public byte ClientLanguages { get; set; }
        public ushort TitleId { get; set; }
        public byte Race { get; set; }
        public byte Tribe { get; set; }
        public byte Gender { get; set; }
        public bool IsFantasiaDetected { get; set; }
        public DateTime AddedAt { get; set; }
        public uint AddedLocationId { get; set; }
        public DateTime LastSeenAt { get; set; }
        public DateTime ArchivedAt { get; set; }
        public Guid? CustomGroupId { get; set; }
        public List<Guid> Tags { get; set; } = new();
        public List<string> PreviousNames { get; set; } = new();
        public string Notes { get; set; } = string.Empty;
        public bool IsArchived { get; set; }
        public bool IsCharacterDeleted { get; set; }
        public bool IsMarkedForRemoval { get; set; }
        public bool IsMissing { get; set; }
        public byte GrandCompany { get; set; }
        public bool IsTrackedForNotifications { get; set; }
    }
}