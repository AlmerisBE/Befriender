namespace Befriender.Core.Characters.Models;

using System;

public class CharacterTag {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
}