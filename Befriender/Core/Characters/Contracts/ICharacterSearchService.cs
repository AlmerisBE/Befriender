namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System.Collections.Generic;

public interface ICharacterSearchService {
    IEnumerable<Character> FilterCharacters(IEnumerable<Character> characters, string searchQuery);
}