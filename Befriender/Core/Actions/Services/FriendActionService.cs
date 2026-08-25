namespace Befriender.Core.Actions.Services;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using System.Collections.Generic;
using System.Linq;

public class FriendActionService : IFriendActionService {
    private List<IFriendAction> registeredActions = new();

    // The constructor accepts an initial collection of actions injected via DI
    public FriendActionService(IEnumerable<IFriendAction> defaultActions) {
        foreach (var action in defaultActions) {
            this.RegisterAction(action);
        }
    }

    public void RegisterAction(IFriendAction action) {
        if (!this.registeredActions.Contains(action)) {
            this.registeredActions.Add(action);
        }
    }

    public IReadOnlyList<IFriendAction> GetAvailableActions(FriendProfile friend) {
        return this.registeredActions.Where(a => a.CanExecute(friend)).ToList();
    }
}