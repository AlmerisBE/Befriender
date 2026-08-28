namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using FFXIVClientStructs.FFXIV.Client.UI;

public class ListToolbarComponent {
    private ILocalizationService loc;

    public ListToolbarComponent(ILocalizationService loc) {
        this.loc = loc;
    }

    public bool Draw(ref bool showOnlineOnly, ref bool groupByGroups, bool showOnlineCheckbox) {
        bool forceRefresh = false;

        if (showOnlineCheckbox) {
            if (ImGui.Checkbox(this.loc.Translate("List_ShowOnlineOnly"), ref showOnlineOnly)) {
                forceRefresh = true;
            }

            ImGui.SameLine();
        }

        if (ImGui.Checkbox(this.loc.Translate("List_GroupByGroups"), ref groupByGroups)) {
            forceRefresh = true;
        }

        ImGui.SameLine();

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

        return forceRefresh;
    }
}