namespace Befriender.Core.Configuration.Models;

using Dalamud.Configuration;
using System;

[Serializable]
public class PluginConfiguration : IPluginConfiguration {
    public int Version { get; set; } = 0;

    public bool ExampleCheckbox { get; set; } = false;

    // Default sync interval in minutes (US-1.3)
    public int SyncIntervalMinutes { get; set; } = 15;
}