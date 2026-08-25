namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using System;
using System.Collections.Generic;

public class FriendStatusBarComponent {
    private IFriendSyncService syncService;
    private ILocalizationService loc;

    public FriendStatusBarComponent(IFriendSyncService syncService, ILocalizationService loc) {
        this.syncService = syncService;
        this.loc = loc;
    }

    public bool Draw(IReadOnlyList<FriendProfile> rawFriends, ref bool showOnlineOnly) {
        bool forceRefresh = false;

        if (ImGui.Checkbox(this.loc.Translate("List_ShowOnlineOnly"), ref showOnlineOnly)) {
            forceRefresh = true;
        }

        ImGui.SameLine();

        int onlineCount = 0, activeCount = 0, archivedCount = 0, deletedCount = 0;
        foreach (var f in rawFriends) {
            if (!f.IsArchived && !f.IsCharacterDeleted) {
                activeCount++;
                if (f.IsOnline) {
                    onlineCount++;
                }
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
            syncText = this.loc.Translate("Status_Scanning");
        }
        else {
            var diff = DateTime.Now - this.syncService.LastSyncTime;
            string timeStr;

            if (diff.TotalDays >= 1) {
                timeStr = this.loc.Translate("Status_DaysAgo", (int)diff.TotalDays);
            }
            else if (diff.TotalHours >= 1) {
                timeStr = this.loc.Translate("Status_HoursAgo", (int)diff.TotalHours);
            }
            else if (diff.TotalMinutes >= 1) {
                timeStr = this.loc.Translate("Status_MinutesAgo", (int)diff.TotalMinutes);
            }
            else {
                timeStr = this.loc.Translate("Status_JustNow");
            }

            syncText = this.loc.Translate("Status_LastSync", timeStr);
        }

        // Generate compact and detailed texts
        var compactText = this.loc.Translate("Status_CompactCounts", syncText, onlineCount, activeCount);
        var tooltipText = this.loc.Translate("Status_TooltipCounts", onlineCount, activeCount, archivedCount, deletedCount, rawFriends.Count);

        var textSize = ImGui.CalcTextSize(compactText);
        var rightAlignPos = ImGui.GetWindowWidth() - textSize.X - (ImGui.GetStyle().WindowPadding.X * 2) - 30.0f;

        ImGui.SetCursorPosX(Math.Max(rightAlignPos, ImGui.GetCursorPosX()));
        ImGui.Text(compactText);

        // Display full breakdown on hover
        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip(tooltipText);
        }

        return forceRefresh;
    }
}