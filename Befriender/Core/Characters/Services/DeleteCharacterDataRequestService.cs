namespace Befriender.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using System;

public class DeleteCharacterDataRequestService : IDeleteCharacterDataRequestService {
    public event Action<Character>? OnDeleteRequested;

    public void RequestDeletion(Character character) {
        this.OnDeleteRequested?.Invoke(character);
    }
}