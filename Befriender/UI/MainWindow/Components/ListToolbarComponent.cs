namespace Befriender.UI.MainWindow.Components;

using Befriender.Core.Configuration.Models;
using Befriender.Core.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using System;
using System.Collections.Generic;

public class ListToolbarComponent {
    private ILocalizationService loc;

    public ListToolbarComponent(ILocalizationService loc) {
        this.loc = loc;
    }

    public bool Draw(
        ref int statusFilter,
        ref bool showNearbyOnly,
        ref bool groupByGroups,
        ref string searchQuery,
        ref bool isFiltersExpanded,
        bool showStatusFilter,
        List<ColumnType> activeColumns) {

        bool forceRefresh = false;

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Filter)) {
            isFiltersExpanded = !isFiltersExpanded;
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip(this.loc.Translate("Tooltip_ToggleFilters"));
        }

        ImGui.SameLine();

        if (showStatusFilter) {
            ImGui.SetNextItemWidth(140f);
            string[] statusOptions = {
                this.loc.Translate("Filter_StatusAll"),
                this.loc.Translate("Filter_StatusOnline"),
                this.loc.Translate("Filter_StatusOffline"),
                this.loc.Translate("Filter_StatusUnsynchronized")
            };

            if (statusFilter < 0 || statusFilter >= statusOptions.Length) {
                statusFilter = 0;
            }

            if (ImGui.Combo("##statusFilter", ref statusFilter, statusOptions, statusOptions.Length)) {
                forceRefresh = true;
            }
            ImGui.SameLine();
        }

        ImGui.SetNextItemWidth(150f);
        if (ImGui.InputTextWithHint("##search", this.loc.Translate("List_SearchHint"), ref searchQuery, 50)) {
            forceRefresh = true;
        }

        ImGui.SameLine();

        if (ImGuiComponents.IconButton(FontAwesomeIcon.Columns)) {
            ImGui.OpenPopup("ColumnSelectorPopup");
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip(this.loc.Translate("Tooltip_SelectColumns"));
        }

        if (ImGui.BeginPopup("ColumnSelectorPopup")) {
            ImGui.TextDisabled(this.loc.Translate("List_VisibleColumns"));
            ImGui.Separator();

            var allColumns = Enum.GetValues<ColumnType>();
            foreach (var col in allColumns) {
                bool isActive = activeColumns.Contains(col);
                if (ImGui.Checkbox(this.loc.Translate($"Column_{col}"), ref isActive)) {
                    if (isActive) {
                        activeColumns.Add(col);
                        activeColumns.Sort();
                    }
                    else if (activeColumns.Count > 1) {
                        activeColumns.Remove(col);
                    }
                    forceRefresh = true;
                }
            }
            ImGui.EndPopup();
        }

        if (isFiltersExpanded) {
            ImGui.Spacing();

            float windowVisibleX2 = ImGui.GetWindowPos().X + ImGui.GetWindowContentRegionMax().X;
            float styleSpacingX = ImGui.GetStyle().ItemSpacing.X;
            float checkboxSquareSize = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemInnerSpacing.X;

            bool isFirstItemOnLine = true;

            void DrawWrappingCheckbox(string label, ref bool value) {
                float itemWidth = checkboxSquareSize + ImGui.CalcTextSize(label).X;

                if (!isFirstItemOnLine) {
                    float nextItemX2 = ImGui.GetItemRectMax().X + styleSpacingX + itemWidth;

                    if (nextItemX2 < windowVisibleX2) {
                        ImGui.SameLine();
                    }
                    else isFirstItemOnLine = true;
                }

                if (ImGui.Checkbox(label, ref value)) {
                    forceRefresh = true;
                }

                isFirstItemOnLine = false;
            }

            DrawWrappingCheckbox(this.loc.Translate("List_ShowNearbyOnly"), ref showNearbyOnly);
            DrawWrappingCheckbox(this.loc.Translate("List_GroupByGroups"), ref groupByGroups);

            ImGui.Spacing();
        }

        return forceRefresh;
    }
}