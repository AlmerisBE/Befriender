namespace Befriender.Core.Friends.Models;

using System;
using System.Collections.Generic;

public class FriendProfile {
    // Unique identity for IPC and character consolidation
    public Guid Id { get; set; } = Guid.Empty;

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
    public bool IsArchived { get; set; } = false;
    public bool IsCharacterDeleted { get; set; } = false;
    public bool IsMarkedForRemoval { get; set; } = false;
    public bool IsMissing { get; set; } = false;
    public byte GrandCompany { get; set; }
    public bool IsTrackedForNotifications { get; set; } = false;
}