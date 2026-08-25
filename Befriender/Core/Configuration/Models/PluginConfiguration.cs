namespace Befriender.Core.Configuration.Models;

using Dalamud.Configuration;
using System;

[Serializable]
public class PluginConfiguration : IPluginConfiguration {
    public int Version { get; set; } = 0;

    public int MinSyncIntervalMinutes { get; set; } = 15;
    public int MaxSyncIntervalMinutes { get; set; } = 30;

    public bool SyncOnLogin { get; set; } = true;
    public bool SyncOnTerritoryChange { get; set; } = true;
    public bool SyncOnFriendListChange { get; set; } = true;
    public int SelectedTheme { get; set; } = 0;
}