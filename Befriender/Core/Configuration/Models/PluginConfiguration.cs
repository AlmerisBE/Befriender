namespace Befriender.Core.Configuration.Models;

using Dalamud.Configuration;
using Dalamud.Game.ClientState.Keys;
using System;

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

    public VirtualKey Hotkey { get; set; } = 0;
    public bool HotkeyCtrl { get; set; } = false;
    public bool HotkeyShift { get; set; } = false;
    public bool HotkeyAlt { get; set; } = false;
}