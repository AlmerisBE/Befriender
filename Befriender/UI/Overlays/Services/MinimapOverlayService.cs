namespace Befriender.UI.Overlays.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Proximity.Contracts;
using Befriender.UI.Overlays.Contracts;
using Befriender.UI.Theme.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Component.GUI;
using System;
using System.Linq;
using System.Numerics;

public unsafe class MinimapOverlayService : IMinimapOverlayService, IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private IGameGui gameGui;
    private IClientState clientState;
    private IObjectTable objectTable;
    private IConfigurationService configService;
    private IProximityService proximityService;
    private ICharacterRegistry registry;
    private IThemeService themeService;

    public MinimapOverlayService(
        IDalamudPluginInterface pluginInterface,
        IGameGui gameGui,
        IClientState clientState,
        IObjectTable objectTable,
        IConfigurationService configService,
        IProximityService proximityService,
        ICharacterRegistry registry,
        IThemeService themeService) {

        this.pluginInterface = pluginInterface;
        this.gameGui = gameGui;
        this.clientState = clientState;
        this.objectTable = objectTable;
        this.configService = configService;
        this.proximityService = proximityService;
        this.registry = registry;
        this.themeService = themeService;

        this.pluginInterface.UiBuilder.Draw += this.DrawOverlay;
    }

    public Vector2 CalculateOverlayPosition(Vector3 localPos, Vector3 friendPos, Vector2 mapCenter, float scale, float maxRadius) {
        float deltaX = friendPos.X - localPos.X;
        float deltaY = friendPos.Z - localPos.Z;

        float scaledX = deltaX * scale;
        float scaledY = deltaY * scale;

        // Calculate the Pythagorean distance from the center
        float distance = (float)Math.Sqrt((scaledX * scaledX) + (scaledY * scaledY));

        // Clamp the position to the edge of the minimap radius if the target is out of bounds
        if (distance > maxRadius) {
            float factor = maxRadius / distance;
            scaledX *= factor;
            scaledY *= factor;
        }

        return new Vector2(mapCenter.X + scaledX, mapCenter.Y + scaledY);
    }

    private void DrawOverlay() {
        if (!this.configService.GetConfig().ShowFriendsOnMinimap) return;
        if (!this.clientState.IsLoggedIn) return;

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) return;

        var addon = this.gameGui.GetAddonByName("NaviMap", 1);
        if (addon.Address == nint.Zero) return;

        var naviMap = (AtkUnitBase*)addon.Address;
        if (!naviMap->IsVisible) return;

        var nearbyIds = this.proximityService.GetNearbyFriendIds();
        if (nearbyIds.Count == 0) return;

        var friends = this.registry.GetAllCharacters().Where(c => nearbyIds.Contains(c.ContentId)).ToList();

        float scale = naviMap->Scale;
        float mapWidth = naviMap->WindowNode->AtkResNode.Width * scale;
        float mapHeight = naviMap->WindowNode->AtkResNode.Height * scale;

        // ImGuiCond.Always is mandatory here to prevent ImGui from saving the position and ignoring dynamic addon movement
        ImGui.SetNextWindowPos(new Vector2(naviMap->X, naviMap->Y), ImGuiCond.Always);
        ImGui.SetNextWindowSize(new Vector2(mapWidth, mapHeight), ImGuiCond.Always);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, Vector2.Zero);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 0f);

        if (ImGui.Begin("Befriender_NaviMapOverlay", ImGuiWindowFlags.NoBackground | ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoFocusOnAppearing | ImGuiWindowFlags.NoSavedSettings)) {
            var drawList = ImGui.GetWindowDrawList();
            var center = new Vector2(naviMap->X + (mapWidth / 2f), naviMap->Y + (mapHeight / 2f));

            float coordinateScale = 2.0f * scale;
            float maxRadius = (mapWidth / 2f) * 0.85f; // Keep the dots slightly inside the native circular border

            var color = ImGui.GetColorU32(this.themeService.CurrentPalette.StatusFallbackOnline);

            foreach (var friend in friends) {
                var pos = this.CalculateOverlayPosition(localPlayer.Position, friend.LastKnownPosition, center, coordinateScale, maxRadius);
                drawList.AddCircleFilled(pos, 4f * scale, color);
            }
        }
        ImGui.End();
        ImGui.PopStyleVar(2);
    }

    public void Dispose() {
        this.pluginInterface.UiBuilder.Draw -= this.DrawOverlay;
    }
}