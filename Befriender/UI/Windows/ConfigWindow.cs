namespace Befriender.UI.Windows;

using Befriender.Core.Configuration.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using System;
using System.Numerics;

public class ConfigWindow : Window {
    private IConfigurationService configurationService;
    private readonly int[] availableIntervals = { 5, 15, 30 };

    public ConfigWindow(IConfigurationService configurationService)
        : base("Befriender Configuration", ImGuiWindowFlags.None) {
        this.configurationService = configurationService;

        this.SizeConstraints = new WindowSizeConstraints {
            MinimumSize = new Vector2(300, 150),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };
    }

    public override void Draw() {
        var config = this.configurationService.GetConfig();
        var currentInterval = config.SyncIntervalMinutes;

        var currentIndex = Array.IndexOf(this.availableIntervals, currentInterval);
        if (currentIndex == -1) {
            currentIndex = 1; // Fallback to 15 minutes if somehow invalid
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