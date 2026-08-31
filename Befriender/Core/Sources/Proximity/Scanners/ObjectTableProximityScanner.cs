namespace Befriender.Core.Sources.Proximity.Scanners;

using Befriender.Core.Characters.Models;
using Befriender.Core.Sources.Proximity.Contracts;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using System.Collections.Generic;

public class ObjectTableProximityScanner : IProximityScanner {
    private IObjectTable objectTable;
    private IClientState clientState;

    public ObjectTableProximityScanner(IObjectTable objectTable, IClientState clientState) {
        this.objectTable = objectTable;
        this.clientState = clientState;
    }

    public IEnumerable<Character> ScanNearbyPlayers() {
        var nearbyPlayers = new List<Character>();
        var localPlayer = this.objectTable.LocalPlayer;

        if (localPlayer == null) {
            return nearbyPlayers;
        }

        uint currentTerritory = this.clientState.TerritoryType;
        uint localCurrentWorld = localPlayer.CurrentWorld.RowId;

        for (int i = 0; i < this.objectTable.Length; i++) {
            var obj = this.objectTable[i];

            if (obj is IPlayerCharacter player && player.Address != localPlayer.Address && player.HomeWorld.RowId > 0 && !string.IsNullOrEmpty(player.Name.TextValue)) {
                nearbyPlayers.Add(new Character {
                    Name = player.Name.TextValue,
                    HomeWorldId = player.HomeWorld.RowId,
                    CurrentWorldId = localCurrentWorld,
                    JobId = (byte)player.ClassJob.RowId,
                    Level = player.Level,
                    LocationId = currentTerritory,
                    IsOnline = true
                });
            }
        }

        return nearbyPlayers;
    }
}