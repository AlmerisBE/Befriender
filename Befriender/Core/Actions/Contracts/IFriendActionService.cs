namespace Befriender.Core.Actions.Contracts;

using Befriender.Core.Friends.Models;
using System.Collections.Generic;

public interface IFriendActionService {
    void RegisterAction(IFriendAction action);
    IReadOnlyList<IFriendAction> GetAvailableActions(FriendProfile friend);
}