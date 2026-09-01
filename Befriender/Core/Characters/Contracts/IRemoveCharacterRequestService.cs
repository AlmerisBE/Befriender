namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System;

public interface IRemoveCharacterRequestService {
    event Action<Character>? OnRemoveRequested;
    void RequestRemoval(Character character);
}