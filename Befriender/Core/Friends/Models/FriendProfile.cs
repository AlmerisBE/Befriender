namespace Befriender.Core.Friends.Models;

public class FriendProfile {
    public ulong ContentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint HomeWorldId { get; set; }
    public bool IsOnline { get; set; }
    public byte JobId { get; set; }
    public ushort LocationId { get; set; }
    public string FcTag { get; set; } = string.Empty;
}