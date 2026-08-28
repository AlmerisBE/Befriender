namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;

public class FriendStatusBarComponent {
    private IFriendSyncService syncService;
    private ILocalizationService loc;
    private IFriendRepository friendRepository;

    public FriendStatusBarComponent(IFriendSyncService syncService, ILocalizationService loc, IFriendRepository friendRepository) {
        this.syncService = syncService;
        this.loc = loc;
        this.friendRepository = friendRepository;
    }

    public void Draw() {
        if (ImGuiComponents.IconButton(FontAwesomeIcon.AddressBook)) {
            unsafe {
                var uiModule = UIModule.Instance();
                if (uiModule != null) {
                    uiModule->ExecuteMainCommand(13);
                }
            }
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip(this.loc.Translate("Tooltip_OpenNativeList"));
        }

        ImGui.SameLine();

        var rawFriends = this.friendRepository.GetFriends();

        int onlineCount = 0, vanillaCount = 0, archivedCount = 0, deletedCount = 0;
        foreach (var f in rawFriends) {
            if (!f.IsArchived) {
                vanillaCount++;
                if (f.IsOnline && !f.IsCharacterDeleted) {
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

        var compactText = this.loc.Translate("Status_CompactCounts", syncText, onlineCount, rawFriends.Count);
        var tooltipText = this.loc.Translate("Status_TooltipCounts", onlineCount, vanillaCount, archivedCount, deletedCount, rawFriends.Count);

        var textSize = ImGui.CalcTextSize(compactText);
        var rightAlignPos = ImGui.GetWindowWidth() - textSize.X - (ImGui.GetStyle().WindowPadding.X * 2);

        ImGui.SetCursorPosX(Math.Max(rightAlignPos, ImGui.GetCursorPosX()));
        ImGui.Text(compactText);

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip(tooltipText);
        }
    }
}