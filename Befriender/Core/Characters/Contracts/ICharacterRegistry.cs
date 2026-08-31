namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;

public interface ICharacterRegistry {
    event Action? RegistryUpdated;

    void RegisterSource(ICharacterSource source);
    void UnregisterSource(Guid sourceId);

    void LoadMasterList(string accountIdentity);
    void SaveMasterList();

    IReadOnlyList<Character> GetAllCharacters();
    IReadOnlyList<Character> GetCharactersBySource(Guid sourceId);
    Character? GetCharacterById(Guid id);
}