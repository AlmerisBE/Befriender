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

            // Calculate the absolute right edge of the visible window content
            float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
            float checkboxSquareSize = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemInnerSpacing.X;

            bool isFirstItemOnLine = true;

            // Local helper function to draw checkboxes with auto-wrapping capabilities
            void DrawWrappingCheckbox(string label, ref bool value) {
                float itemWidth = checkboxSquareSize + ImGui.CalcTextSize(label).X;

                if (!isFirstItemOnLine) {
                    float nextItemX2 = ImGui.GetCursorScreenPos().X + itemWidth;

                    if (nextItemX2 < windowVisibleX2) {
                        ImGui.SameLine();
                    }
                    else {
                        isFirstItemOnLine = true;
                    }
                }

                if (ImGui.Checkbox(label, ref value)) {
                    forceRefresh = true;
                }

                isFirstItemOnLine = false;
            }

            if (showOnlineCheckbox) {
                DrawWrappingCheckbox(this.loc.Translate("List_ShowOnlineOnly"), ref showOnlineOnly);
            }

            DrawWrappingCheckbox(this.loc.Translate("List_ShowNearbyOnly"), ref showNearbyOnly);
            DrawWrappingCheckbox(this.loc.Translate("List_GroupByGroups"), ref groupByGroups);

            ImGui.Spacing();
        }

        return forceRefresh;
    }
}