namespace Befriender.Core.Friends.Models;

using System;

public class FriendGroup {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}