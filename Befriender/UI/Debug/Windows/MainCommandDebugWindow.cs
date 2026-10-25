namespace Befriender.UI.Debug.Windows;

using Befriender.Core.GameData.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Linq;
using System.Numerics;

public class MainCommandDebugWindow : Window {
    private IMainCommandProvider commandProvider;
    private string searchFilter = string.Empty;

    public MainCommandDebugWindow(IMainCommandProvider commandProvider)
        : base("Debug: Main Commands###Befriender_MainCommands", ImGuiWindowFlags.NoCollapse) {

        this.commandProvider = commandProvider;
        this.Size = new Vector2(600, 400);
        this.SizeCondition = ImGuiCond.FirstUseEver;
    }

    public override void Draw() {
        ImGui.InputText("Search", ref this.searchFilter, 100);
        ImGui.Separator();

        var flags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY | ImGuiTableFlags.Resizable;

        if (ImGui.BeginTable("MainCommandsTable", 3, flags)) {
            ImGui.TableSetupColumn("ID", ImGuiTableColumnFlags.WidthFixed, 50f);
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed, 200f);
            ImGui.TableSetupColumn("Description", ImGuiTableColumnFlags.WidthStretch);
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            var commands = this.commandProvider.GetAllMainCommands();

            if (!string.IsNullOrWhiteSpace(this.searchFilter)) {
                string filter = this.searchFilter.ToLowerInvariant();
                commands = commands.Where(c =>
                    c.Name.Contains(filter, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(filter, StringComparison.OrdinalIgnoreCase));
            }

            foreach (var cmd in commands) {
                ImGui.TableNextRow();

                ImGui.TableNextColumn();
                ImGui.Text(cmd.Id.ToString());

                ImGui.TableNextColumn();
                ImGui.Text(cmd.Name);

                ImGui.TableNextColumn();
                ImGui.TextDisabled(cmd.Description);

                // Allow copying the ID by clicking on the row
                if (ImGui.IsItemClicked()) {
                    ImGui.SetClipboardText(cmd.Id.ToString());
                }
                if (ImGui.IsItemHovered()) {
                    ImGui.SetTooltip("Click to copy ID");
                }
            }

            ImGui.EndTable();
        }
    }
}