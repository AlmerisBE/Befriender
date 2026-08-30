namespace Befriender.Core.Characters.Models;

using System;
using System.Collections.Generic;

public class Character {
    public ulong ContentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint HomeWorldId { get; set; }
    public uint CurrentWorldId { get; set; }
    public byte JobId { get; set; }
    public byte Level { get; set; }
    public uint LocationId { get; set; }
    public bool IsOnline { get; set; }
    public string FcTag { get; set; } = string.Empty;

    // Identifies which sources currently "see" or know this character (e.g., "FriendList", "Proximity", "Party")
    public HashSet<string> ActiveSources { get; set; } = new(StringComparer.OrdinalIgnoreCase);

    // Determines if the character shares the exact identity with another
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