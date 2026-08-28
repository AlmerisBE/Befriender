namespace Befriender.UI.FriendList.Components;

using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;

public class ListToolbarComponent {
    private ILocalizationService loc;

    public ListToolbarComponent(ILocalizationService loc) {
        this.loc = loc;
    }

    public bool Draw(ref bool showOnlineOnly, ref bool groupByGroups, ref string searchQuery, bool showOnlineCheckbox) {
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

        ImGui.SetNextItemWidth(150f);
        if (ImGui.InputTextWithHint("##search", this.loc.Translate("List_SearchHint"), ref searchQuery, 50)) {
            forceRefresh = true;
        }

        return forceRefresh;
    }
}