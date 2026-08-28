namespace Befriender.Core.Friends.Models;

using System;

public class FriendTag {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}