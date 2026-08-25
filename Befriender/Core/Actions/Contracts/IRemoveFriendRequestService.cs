namespace Befriender.Core.Actions.Contracts;

using Befriender.Core.Friends.Models;
using System;

public interface IRemoveFriendRequestService {
    event Action<FriendProfile>? OnRemoveRequested;
    void RequestRemoval(FriendProfile friend);
}