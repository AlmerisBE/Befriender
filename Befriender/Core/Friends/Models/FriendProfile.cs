namespace Befriender.Core.Friends.Models;

public class FriendProfile {
    public ulong ContentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public uint HomeWorldId { get; set; }
}