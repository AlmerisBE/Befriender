namespace Befriender.Core.Friends.Models;

using System;
using System.Collections.Generic;

public class FriendProfile {
    public ulong ContentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint HomeWorldId { get; set; }
    public uint CurrentWorldId { get; set; }
    public bool IsOnline { get; set; }
    public byte JobId { get; set; }
    public ushort LocationId { get; set; }
    public string FcTag { get; set; } = string.Empty;

    // Holds the bitmask from CharacterData.OnlineStatus
    public ulong OnlineStateMask { get; set; }

    // Holds the bitmask for the client languages (JA, EN, DE, FR)
    public byte ClientLanguages { get; set; }

    public DateTime AddedAt { get; set; }
    public ushort AddedLocationId { get; set; }
    public DateTime LastSeenAt { get; set; }

    // History of previous names detected for this character
    public List<string> PreviousNames { get; set; } = new();

    public string Notes { get; set; } = string.Empty;
    public bool IsArchived { get; set; } = false;
}