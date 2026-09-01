namespace Befriender.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using System.Collections.Generic;
using System.Linq;

public class CharacterActionService : ICharacterActionService {
    private List<ICharacterAction> registeredActions = new();

    public CharacterActionService(IEnumerable<ICharacterAction> defaultActions) {
        foreach (var action in defaultActions) {
            this.RegisterAction(action);
        }
    }

    public void RegisterAction(ICharacterAction action) {
        if (!this.registeredActions.Contains(action)) {
            this.registeredActions.Add(action);
        }
    }

    public IReadOnlyList<ICharacterAction> GetAvailableActions(Character character) {
        return this.registeredActions.Where(a => a.CanExecute(character)).ToList();
    }
}