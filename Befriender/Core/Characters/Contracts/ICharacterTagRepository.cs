namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;

public interface ICharacterTagRepository {
    event Action? CacheCleared;
    IReadOnlyList<CharacterTag> GetTags();
    void AddTag(string name);
    void UpdateTag(CharacterTag tag);
    void RemoveTag(Guid id);
    void Save();
    void ClearCache();
}