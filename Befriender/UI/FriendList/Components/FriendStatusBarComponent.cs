namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Client.UI;
using System;
using System.Collections.Generic;
using System.Linq;

public class FriendStatusBarComponent {
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private Guid friendSourceId;

    public FriendStatusBarComponent(ICharacterRegistry registry, IEnumerable<ICharacterSource> sources, ILocalizationService loc) {
        this.registry = registry;
        this.loc = loc;

        var friendSource = sources.FirstOrDefault(s => s.Name == "FriendList");
        if (friendSource != null) {
            this.friendSourceId = friendSource.SourceId;
        }
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
            if (c.ActiveSourceIds.Contains(this.friendSourceId)) {
                vanillaCount++;
                if (c.IsOnline && !string.IsNullOrEmpty(c.Name)) {
                    onlineCount++;
                }
            }
            if (!c.IsActivelyTracked && !string.IsNullOrEmpty(c.Name)) {
                archivedCount++;
            }

            if (string.IsNullOrEmpty(c.Name)) {
                deletedCount++;
            }
        }

        var compactText = this.loc.Translate("Status_CompactCounts", "Befriender", onlineCount, allCharacters.Count);
        var tooltipText = this.loc.Translate("Status_TooltipCounts", onlineCount, vanillaCount, archivedCount, deletedCount, allCharacters.Count);

        var textSize = ImGui.CalcTextSize(compactText);
        var rightAlignPos = ImGui.GetWindowWidth() - textSize.X - (ImGui.GetStyle().WindowPadding.X * 2);

        ImGui.SetCursorPosX(Math.Max(rightAlignPos, ImGui.GetCursorPosX()));
        ImGui.Text(compactText);

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip(tooltipText);
        }
    }
}