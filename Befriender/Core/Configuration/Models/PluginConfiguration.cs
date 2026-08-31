namespace Befriender.Core.Configuration.Models;

using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using System;
using System.Collections.Generic;

[Serializable]
public class PluginConfiguration : IPluginConfiguration {
    public int Version { get; set; } = 0;

    public int MinSyncIntervalMinutes { get; set; } = 15;
    public int MaxSyncIntervalMinutes { get; set; } = 30;

    public bool SyncOnLogin { get; set; } = true;
    public bool SyncOnTerritoryChange { get; set; } = true;
    public bool SyncOnFriendListChange { get; set; } = true;

    public string SelectedThemeName { get; set; } = "Dark";

    public bool IsProfilePanelOpen { get; set; } = false;
    public bool GroupByCustomGroups { get; set; } = false; // Legacy fallback

    public VirtualKey Hotkey { get; set; } = 0;
    public bool HotkeyCtrl { get; set; } = false;
    public bool HotkeyShift { get; set; } = false;
    public bool HotkeyAlt { get; set; } = false;

    public bool EnableProximityDetection { get; set; } = true;
    public bool NotifyOnNearbyFriends { get; set; } = true;
    public bool NotifyOnNearbyArchived { get; set; } = false;

    public Dictionary<string, TabState> TabStates { get; set; } = new();
}