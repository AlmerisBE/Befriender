namespace Befriender.UI.Configuration.Tabs;

using Befriender.Core.Configuration.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using System;

public class ConfigTab : ITab {
    private IConfigurationService configurationService;
    private readonly int[] availableIntervals = { 5, 15, 30 };

    public string Name => "Configuration";

    public ConfigTab(IConfigurationService configurationService) {
        this.configurationService = configurationService;
    }

    public void Draw() {
        var config = this.configurationService.GetConfig();
        var currentInterval = config.SyncIntervalMinutes;

        var currentIndex = Array.IndexOf(this.availableIntervals, currentInterval);
        if (currentIndex == -1) {
            currentIndex = 1;
        }

        var previewValue = $"{this.availableIntervals[currentIndex]} minutes";

        if (ImGui.BeginCombo("Background Sync Interval", previewValue)) {
            for (int i = 0; i < this.availableIntervals.Length; i++) {
                bool isSelected = currentIndex == i;
                if (ImGui.Selectable($"{this.availableIntervals[i]} minutes", isSelected)) {
                    config.SyncIntervalMinutes = this.availableIntervals[i];
                    this.configurationService.Save();
                }

                if (isSelected) {
                    ImGui.SetItemDefaultFocus();
                }
            }
            ImGui.EndCombo();
        }
    }
}