namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System.Collections.Generic;

public interface ICharacterStorage {
    IEnumerable<Character> Load(string storeName, string accountIdentity);
    void Save(string storeName, string accountIdentity, IEnumerable<Character> characters);
}