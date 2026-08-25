namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;

public class FriendStatusBarComponent {
    private IFriendSyncService syncService;

    public FriendStatusBarComponent(IFriendSyncService syncService) {
        this.syncService = syncService;
    }

    public bool Draw(IReadOnlyList<FriendProfile> rawFriends, ref bool showOnlineOnly) {
        bool forceRefresh = false;

        if (ImGui.Checkbox("Show Online Only", ref showOnlineOnly)) {
            forceRefresh = true;
        }

        ImGui.SameLine();

        int onlineCount = 0, archivedCount = 0, deletedCount = 0;
        foreach (var f in rawFriends) {
            if (f.IsOnline && !f.IsArchived && !f.IsCharacterDeleted) {
                onlineCount++;
            }

            if (f.IsArchived) {
                archivedCount++;
            }

            if (f.IsCharacterDeleted) {
                deletedCount++;
            }
        }

        string syncText;
        if (this.syncService.IsSyncPending || this.syncService.LastSyncTime == DateTime.MinValue) {
            syncText = "Scanning...";
        }
        else {
            var diff = DateTime.Now - this.syncService.LastSyncTime;
            string timeStr;

            if (diff.TotalDays >= 1) {
                timeStr = $"{(int)diff.TotalDays}d ago";
            }
            else if (diff.TotalHours >= 1) {
                timeStr = $"{(int)diff.TotalHours}h ago";
            }
            else if (diff.TotalMinutes >= 1) {
                timeStr = $"{(int)diff.TotalMinutes}m ago";
            }
            else {
                timeStr = "Just now";
            }

            syncText = $"Last Sync: {timeStr}";
        }

        var statusText = $"{syncText} | Online: {onlineCount} | Archived: {archivedCount} | Deleted: {deletedCount} | Total: {rawFriends.Count}";
        var textSize = ImGui.CalcTextSize(statusText);
        var rightAlignPos = ImGui.GetWindowWidth() - textSize.X - (ImGui.GetStyle().WindowPadding.X * 2) - 30.0f;

        ImGui.SetCursorPosX(Math.Max(rightAlignPos, ImGui.GetCursorPosX()));
        ImGui.Text(statusText);

        return forceRefresh;
    }
}