namespace Befriender.UI.MainWindow.Components;

using Befriender.Core.Characters.Contracts;
using Befriender.UI.Localization.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using System;
using System.Linq;

public class FriendStatusBarComponent : IDisposable {
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private DateTime lastUpdateTime;

    public FriendStatusBarComponent(ICharacterRegistry registry, ILocalizationService loc) {
        this.registry = registry;
        this.loc = loc;
        this.lastUpdateTime = DateTime.Now;

        this.registry.RegistryUpdated += this.OnRegistryUpdated;
    }

    private void OnRegistryUpdated() {
        this.lastUpdateTime = DateTime.Now;
    }

    public void Draw() {
        // Draw the contextual actions menu icon on the far left
        if (ImGuiComponents.IconButton(FontAwesomeIcon.AddressBook)) {
            // Future implementation for quick actions or settings
        }

        if (ImGui.IsItemHovered()) {
            ImGui.SetTooltip(this.loc.Translate("Tooltip_QuickActions"));
        }

        var allCharacters = this.registry.GetAllCharacters();
        int total = allCharacters.Count;
        int online = allCharacters.Count(c => c.IsOnline);

        var elapsed = DateTime.Now - this.lastUpdateTime;

        // Format the elapsed time to be user-friendly (e.g., "2m 15s" or "45s")
        string timeString = elapsed.TotalMinutes >= 1
            ? $"{(int)elapsed.TotalMinutes}m {elapsed.Seconds}s"
            : $"{(int)elapsed.TotalSeconds}s";

        string statusText = this.loc.Translate("StatusBar_Status", timeString, online, total);

        float textWidth = ImGui.CalcTextSize(statusText).X;
        float availableWidth = ImGui.GetContentRegionAvail().X;

        // Push the text to the absolute right of the available space on the same line
        ImGui.SameLine(Math.Max(0, availableWidth - textWidth + ImGui.GetCursorPosX()));
        ImGui.TextDisabled(statusText);
    }

    public void Dispose() {
        this.registry.RegistryUpdated -= this.OnRegistryUpdated;
    }
}