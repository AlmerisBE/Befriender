namespace Befriender.Core.Friends.Contracts;

using System;

public interface IFriendSyncService {
    DateTime LastSyncTime { get; }
    bool IsSyncPending { get; }
    bool IsWindowOpen { get; set; }

    void ForceSync();
    void RequestServerRefresh();
    void RequestCrossWorldRefresh();
}