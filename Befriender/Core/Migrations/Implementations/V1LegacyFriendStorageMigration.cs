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

        // The migration is idempotent: if the file is already migrated (renamed to .bak) or doesn't exist, we skip
        if (!File.Exists(legacyPath)) {
            return;
        }

        string json = File.ReadAllText(legacyPath);
        var legacyProfiles = JsonSerializer.Deserialize<List<LegacyFriendProfile>>(json);

        if (legacyProfiles != null && legacyProfiles.Count > 0) {
            var characters = new List<Character>();

            foreach (var profile in legacyProfiles) {
                var character = new Character {
                    Id = profile.Id != Guid.Empty ? profile.Id : Guid.NewGuid(),
                    ContentId = profile.ContentId,
                    Name = profile.Name ?? string.Empty,
                    HomeWorldId = profile.HomeWorldId,
                    CurrentWorldId = profile.CurrentWorldId,
                    JobId = profile.JobId,
                    Level = profile.Level,
                    LocationId = profile.LocationId,
                    IsOnline = profile.IsOnline,
                    FcTag = profile.FcTag ?? string.Empty
                };

                character.ActiveSourceIds.Add(Guid.Parse("A1B2C3D4-E5F6-4A7B-8C9D-E0F1A2B3C4D5")); // FriendList SourceId

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
                character.CustomProperties["Befriender_Tags"] = JsonSerializer.Serialize(profile.Tags ?? new List<Guid>());
                character.CustomProperties["Befriender_PreviousNames"] = JsonSerializer.Serialize(profile.PreviousNames ?? new List<string>());
                character.CustomProperties["Befriender_Notes"] = profile.Notes ?? string.Empty;
                character.CustomProperties["Befriender_IsArchived"] = profile.IsArchived.ToString();
                character.CustomProperties["Befriender_IsCharacterDeleted"] = profile.IsCharacterDeleted.ToString();
                character.CustomProperties["Befriender_IsMarkedForRemoval"] = profile.IsMarkedForRemoval.ToString();
                character.CustomProperties["Befriender_IsMissing"] = profile.IsMissing.ToString();
                character.CustomProperties["Befriender_GrandCompany"] = profile.GrandCompany.ToString();
                character.CustomProperties["Befriender_IsTrackedForNotifications"] = profile.IsTrackedForNotifications.ToString();

                characters.Add(character);
            }

            this.characterStorage.Save("FriendList", accountIdentity, characters);
        }

        File.Move(legacyPath, $"{legacyPath}.bak");
    }

    // Private internal class representing the exact legacy schema
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