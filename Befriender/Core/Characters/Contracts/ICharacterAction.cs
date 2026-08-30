namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using System;

public interface ICharacterAction {
    Guid ActionId { get; }
    string InternalName { get; }
    FontAwesomeIcon Icon { get; }

    bool CanExecute(Character character);
    void Execute(Character character);
}