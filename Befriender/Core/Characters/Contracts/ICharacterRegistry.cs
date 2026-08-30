namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;

public interface ICharacterRegistry {
    event Action? RegistryUpdated;

    void RegisterSource(ICharacterSource source);
    void UnregisterSource(Guid sourceId);

    IReadOnlyList<Character> GetConsolidatedCharacters();
    Character? GetCharacterById(Guid id);
}