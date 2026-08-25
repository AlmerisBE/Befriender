namespace Befriender.Core.Actions.Services;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using System;

public class RemoveFriendRequestService : IRemoveFriendRequestService {
    public event Action<FriendProfile>? OnRemoveRequested;

    public void RequestRemoval(FriendProfile friend) {
        this.OnRemoveRequested?.Invoke(friend);
    }
}