namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;

public interface ICharacterSource {
    Guid SourceId { get; }
    string Name { get; }
    int Priority { get; }

    event Action? DataUpdated;

    IEnumerable<Character> GetCurrentState();
    void RequestManualRefresh();
}