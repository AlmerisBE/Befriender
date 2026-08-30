namespace Befriender.Core.Characters.Models;

using System;
using System.Collections.Generic;

public class Character {
    public Guid Id { get; set; } = Guid.NewGuid();
    public ulong ContentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint HomeWorldId { get; set; }
    public uint CurrentWorldId { get; set; }
    public byte JobId { get; set; }
    public byte Level { get; set; }
    public uint LocationId { get; set; }
    public bool IsOnline { get; set; }
    public string FcTag { get; set; } = string.Empty;

    // Core MMO and Social Properties elevated to the universal model[cite: 1]
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

    public HashSet<Guid> ActiveSourceIds { get; set; } = new();
    public Dictionary<string, string> CustomProperties { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    public bool IsSameIdentity(ulong otherContentId, string otherName, uint otherHomeWorldId) {
        if (this.ContentId > 0 && otherContentId > 0 && this.ContentId == otherContentId) {
            return true;
        }

        if (this.HomeWorldId == otherHomeWorldId && string.Equals(this.Name, otherName, StringComparison.OrdinalIgnoreCase)) {
            return true;
        }

        return false;
    }
}