namespace Befriender.Core.Characters.Models;

using System;

public class CharacterGroup {
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}