namespace Befriender.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using System;

public class RemoveCharacterRequestService : IRemoveCharacterRequestService {
    public event Action<Character>? OnRemoveRequested;

    public void RequestRemoval(Character character) {
        this.OnRemoveRequested?.Invoke(character);
    }
}