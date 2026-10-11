namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System;

public interface IDeleteCharacterDataRequestService {
    event Action<Character>? OnDeleteRequested;
    void RequestDeletion(Character character);
}