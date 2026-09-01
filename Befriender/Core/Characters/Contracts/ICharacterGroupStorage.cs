namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System.Collections.Generic;

public interface ICharacterGroupStorage {
    IReadOnlyList<CharacterGroup> Load(string characterId);
    void Save(string characterId, IEnumerable<CharacterGroup> groups);
}