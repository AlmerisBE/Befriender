namespace Befriender.Core.Friends.Contracts;

using System;

public interface IFriendSyncService {
    DateTime LastSyncTime { get; }
    void ForceSync();
}