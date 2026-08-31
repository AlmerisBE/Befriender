namespace Befriender.UI.MainWindow.Components;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public class FriendStatusBarComponent : IDisposable {
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private ICharacterSource? friendSource;

    private DateTime lastSyncTime = DateTime.MinValue;

    public FriendStatusBarComponent(ICharacterRegistry registry, IEnumerable<ICharacterSource> sources, ILocalizationService loc) {
        this.registry = registry;
        this.loc = loc;

        this.friendSource = sources.FirstOrDefault(s => s.Name == "FriendList");
        this.registry.RegistryUpdated += this.OnRegistryUpdated;
    }

    private void OnRegistryUpdated() {
        this.lastSyncTime = DateTime.Now;
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

        var allCharacters = this.registry.GetAllCharacters();

        int onlineCount = 0, vanillaCount = 0, archivedCount = 0, deletedCount = 0;
        foreach (var c in allCharacters) {
            bool isDeleted = string.IsNullOrEmpty(c.Name);
            bool isVanilla = this.friendSource != null && c.ActiveSourceIds.Contains(this.friendSource.SourceId);

            if (isVanilla && !isDeleted) {
                vanillaCount++;
                if (c.IsOnline) {
                    onlineCount++;
                }
            }
            if (!c.IsActivelyTracked && !isDeleted) {
                archivedCount++;
            }

            if (isDeleted) {
                deletedCount++;
            }
        }

        // On consulte dynamiquement la source concernée
        bool isCurrentlySyncing = (this.friendSource != null && this.friendSource.IsSyncing) || this.lastSyncTime == DateTime.MinValue;

        string syncText;
        if (isCurrentlySyncing) {
            syncText = this.loc.Translate("Status_Scanning");
        }
        else {
            var diff = DateTime.Now - this.lastSyncTime;
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

        var compactText = this.loc.Translate("Status_CompactCounts", syncText, onlineCount, allCharacters.Count);
        var tooltipText = this.loc.Translate("Status_TooltipCounts", onlineCount, vanillaCount, archivedCount, deletedCount, allCharacters.Count);

        var textSize = ImGui.CalcTextSize(compactText);
        var rightAlignPos = ImGui.GetWindowWidth() - textSize.X - (ImGui.GetStyle().WindowPadding.X * 2);

        ImGui.SetCursorPosX(Math.Max(rightAlignPos, ImGui.GetCursorPosX()));
        ImGui.Text(compactText);

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip(tooltipText);
        }
    }

    public void Dispose() {
        this.registry.RegistryUpdated -= this.OnRegistryUpdated;
    }
}