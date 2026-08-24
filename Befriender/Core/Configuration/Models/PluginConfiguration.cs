namespace Befriender.Core.Configuration.Models;

using Dalamud.Configuration;
using System;

[Serializable]
public class PluginConfiguration : IPluginConfiguration {
    public int Version { get; set; } = 0;

    public int SyncIntervalMinutes { get; set; } = 15;

    public bool SyncOnLogin { get; set; } = true;
    public bool SyncOnTerritoryChange { get; set; } = true;
    public bool SyncOnFriendListChange { get; set; } = true;
}