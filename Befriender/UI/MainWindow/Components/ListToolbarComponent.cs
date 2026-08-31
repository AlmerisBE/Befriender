namespace Befriender.UI.MainWindow.Components;

using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;

public class ListToolbarComponent {
    private ILocalizationService loc;
    private bool isFiltersExpanded = false;

    public ListToolbarComponent(ILocalizationService loc) {
        this.loc = loc;
    }

    public bool Draw(ref bool showOnlineOnly, ref bool showNearbyOnly, ref bool groupByGroups, ref string searchQuery, bool showOnlineCheckbox) {
        bool forceRefresh = false;

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Filter)) {
            this.isFiltersExpanded = !this.isFiltersExpanded;
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip(this.loc.Translate("Tooltip_ToggleFilters"));
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(150f);
        if (ImGui.InputTextWithHint("##search", this.loc.Translate("List_SearchHint"), ref searchQuery, 50)) {
            forceRefresh = true;
        }

        if (this.isFiltersExpanded) {
            ImGui.Spacing();

            if (showOnlineCheckbox) {
                if (ImGui.Checkbox(this.loc.Translate("List_ShowOnlineOnly"), ref showOnlineOnly)) {
                    forceRefresh = true;
                }

                ImGui.SameLine();
            }

            if (ImGui.Checkbox(this.loc.Translate("List_ShowNearbyOnly"), ref showNearbyOnly)) {
                forceRefresh = true;
            }

            ImGui.SameLine();

            if (ImGui.Checkbox(this.loc.Translate("List_GroupByGroups"), ref groupByGroups)) {
                forceRefresh = true;
            }

            ImGui.Spacing();
        }

        return forceRefresh;
    }
}