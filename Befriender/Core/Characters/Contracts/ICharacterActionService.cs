namespace Befriender.Core.Characters.Contracts;

using Befriender.Core.Characters.Models;
using System.Collections.Generic;

public interface ICharacterActionService {
    void RegisterAction(ICharacterAction action);
    IReadOnlyList<ICharacterAction> GetAvailableActions(Character character);
}