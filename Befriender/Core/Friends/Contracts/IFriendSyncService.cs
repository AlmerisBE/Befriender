namespace Befriender.Core.Friends.Contracts;

using System;

public interface IFriendSyncService {
    DateTime LastSyncTime { get; }
    bool IsSyncPending { get; }
    void ForceSync();
    void RequestServerRefresh();
}