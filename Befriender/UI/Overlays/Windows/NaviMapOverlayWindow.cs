namespace Befriender.UI.Overlays.Windows;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Proximity.Contracts;
using Befriender.UI.Theme.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Linq;
using System.Numerics;

public unsafe class NaviMapOverlayWindow : Window {
    private IGameGui gameGui;
    private IClientState clientState;
    private IObjectTable objectTable;
    private IConfigurationService configService;
    private IProximityService proximityService;
    private ICharacterRegistry registry;
    private IThemeService themeService;
    private IGameDataService gameDataService;
    private IPluginLog pluginLog;

    public NaviMapOverlayWindow(
        IGameGui gameGui,
        IClientState clientState,
        IObjectTable objectTable,
        IConfigurationService configService,
        IProximityService proximityService,
        ICharacterRegistry registry,
        IThemeService themeService,
        IGameDataService gameDataService,
        IPluginLog pluginLog)
        : base("Befriender_NaviMapOverlay", ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoBringToFrontOnFocus | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoNavFocus | ImGuiWindowFlags.NoSavedSettings) {

        this.gameGui = gameGui;
        this.clientState = clientState;
        this.objectTable = objectTable;
        this.configService = configService;
        this.proximityService = proximityService;
        this.registry = registry;
        this.themeService = themeService;
        this.gameDataService = gameDataService;
        this.pluginLog = pluginLog;

        this.ForceMainWindow = true;
        this.IsOpen = true;
        this.RespectCloseHotkey = false;

        this.PositionCondition = ImGuiCond.Always;
        this.SizeCondition = ImGuiCond.Always;
    }

    public override bool DrawConditions() {
        if (!this.IsOpen) this.IsOpen = true;

        if (!this.configService.GetConfig().ShowFriendsOnMinimap) return false;
        if (!this.clientState.IsLoggedIn) return false;
        if (this.objectTable.LocalPlayer == null) return false;

        var addon = this.gameGui.GetAddonByName("_NaviMap", 1);
        if (addon.Address == nint.Zero) return false;

        var naviMap = (AddonNaviMap*)addon.Address;
        if (!naviMap->AtkUnitBase.IsVisible) return false;
        if (naviMap->AtkUnitBase.RootNode == null) return false;

        return true;
    }

    public override void PreDraw() {
        var addon = this.gameGui.GetAddonByName("_NaviMap", 1);
        if (addon.Address == nint.Zero) return;

        var naviMap = (AddonNaviMap*)addon.Address;
        if (naviMap->AtkUnitBase.RootNode == null) return;

        float scale = naviMap->AtkUnitBase.Scale;
        float mapWidth = 218f * scale;
        float mapHeight = 218f * scale;

        this.Position = new Vector2(naviMap->AtkUnitBase.X, naviMap->AtkUnitBase.Y);
        this.Size = new Vector2(mapWidth, mapHeight);
    }

    public Vector2 CalculateOverlayPosition(Vector3 localPos, Vector3 friendPos, Vector2 mapCenter, float rotation, float zoom, float naviScale, float sizeFactor, float maxRadius, bool isLocked) {
        Vector2 relativePos = new Vector2(localPos.X - friendPos.X, localPos.Z - friendPos.Z);

        float zoneScale = sizeFactor / 100f;
        if (zoneScale <= 0f) zoneScale = 1f;
        if (zoom <= 0f) zoom = 1f;

        float offsetMultiplier = zoneScale * zoom * naviScale * 0.5f;

        relativePos *= offsetMultiplier;
        Vector2 pos = mapCenter - relativePos;

        if (!isLocked) {
            float cosTheta = MathF.Cos(rotation);
            float sinTheta = MathF.Sin(rotation);

            float rotatedX = (cosTheta * (pos.X - mapCenter.X)) - (sinTheta * (pos.Y - mapCenter.Y)) + mapCenter.X;
            float rotatedY = (sinTheta * (pos.X - mapCenter.X)) + (cosTheta * (pos.Y - mapCenter.Y)) + mapCenter.Y;

            pos = new Vector2(rotatedX, rotatedY);
        }

        float distance = Vector2.Distance(mapCenter, pos);
        if (distance > maxRadius) {
            Vector2 originToObject = pos - mapCenter;
            originToObject *= maxRadius / distance;
            pos = mapCenter + originToObject;
        }

        return pos;
    }

    public override void Draw() {
        var nearbyIds = this.proximityService.GetNearbyFriendIds();
        if (nearbyIds.Count == 0) {
            return;
        }

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) return;

        var addon = this.gameGui.GetAddonByName("_NaviMap", 1);
        if (addon.Address == nint.Zero) return;

        var naviMap = (AddonNaviMap*)addon.Address;
        if (naviMap->AtkUnitBase.RootNode == null) return;

        float scale = naviMap->AtkUnitBase.Scale;
        float mapWidth = 218f * scale;

        float mapX = naviMap->AtkUnitBase.X;
        float mapY = naviMap->AtkUnitBase.Y;

        var drawList = ImGui.GetWindowDrawList();
        var center = new Vector2(mapX + (mapWidth / 2f), mapY + (mapWidth / 2f));

        center.Y -= 5f * scale;

        float rotation = 0f;
        float zoom = 1f;
        bool isLocked = false;

        try {
            var lockNode = naviMap->AtkUnitBase.GetNodeById(4);
            if (lockNode != null && lockNode->Type == NodeType.Component) {
                var checkBox = (AtkComponentCheckBox*)((AtkComponentNode*)lockNode)->Component;
                if (checkBox != null) isLocked = checkBox->IsChecked;
            }

            if (naviMap->AtkUnitBase.UldManager.LoadedState == AtkLoadState.Loaded) {
                var rotNode = naviMap->AtkUnitBase.GetNodeById(8);
                if (rotNode != null) {
                    rotation = rotNode->Rotation;
                }

                var mapComponentNode = naviMap->AtkUnitBase.GetNodeById(18);
                if (mapComponentNode != null && mapComponentNode->Type == NodeType.Component) {
                    var comp = ((AtkComponentNode*)mapComponentNode)->Component;
                    if (comp != null) {
                        var imageNode = comp->UldManager.SearchNodeById(6);
                        if (imageNode != null) zoom = imageNode->ScaleX;
                    }
                }
            }
        }
        catch {
        }

        float sizeFactor = this.gameDataService.GetMapSizeFactor(AgentMap.Instance()->CurrentMapId);
        float maxRadius = mapWidth * 0.315f;

        var color = ImGui.GetColorU32(this.themeService.CurrentPalette.StatusFallbackOnline);
        var friends = this.registry.GetAllCharacters().Where(c => nearbyIds.Contains(c.ContentId));

        foreach (var friend in friends) {
            if (friend.LastKnownPosition == Vector3.Zero) continue;

            var pos = this.CalculateOverlayPosition(localPlayer.Position, friend.LastKnownPosition, center, rotation, zoom, scale, sizeFactor, maxRadius, isLocked);

            drawList.AddCircleFilled(pos, 4f * scale, color);
            drawList.AddCircle(pos, 4f * scale, ImGui.GetColorU32(new Vector4(0f, 0f, 0f, 0.7f)), 0, 1.5f * scale);
        }

    }
}