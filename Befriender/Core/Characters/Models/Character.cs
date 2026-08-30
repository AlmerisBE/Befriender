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

    // Identifies which sources currently provide data for this character
    public HashSet<Guid> ActiveSourceIds { get; set; } = new();

    // Allows internal features or third-party plugins to inject arbitrary key-value metadata
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