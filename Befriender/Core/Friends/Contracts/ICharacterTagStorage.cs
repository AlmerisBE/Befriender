namespace Befriender.Core.Friends.Contracts;

using Befriender.Core.Characters.Models;
using System.Collections.Generic;

public interface ICharacterTagStorage {
    IEnumerable<CharacterTag> Load(string characterId);
    void Save(string characterId, IEnumerable<CharacterTag> tags);
}