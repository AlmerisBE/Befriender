namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;

public interface ICharacterSource {
    string SourceId { get; }

    // Higher priority sources will overwrite the data of lower priority sources during consolidation.
    // For example, Proximity (rendering) should overwrite FriendList (cached server data).
    int Priority { get; }

    bool IsEnabled { get; set; }

    event Action? DataUpdated;

    IEnumerable<Character> GetCharacters();
}