namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;

public interface ICharacterGroupRepository {
    event Action? CacheCleared;
    IReadOnlyList<CharacterGroup> GetGroups();
    void AddGroup(string title);
    void UpdateGroup(CharacterGroup group);
    void RemoveGroup(Guid id);
    void MoveGroupUp(Guid id);
    void MoveGroupDown(Guid id);
    void Save();
    void ClearCache();
}