namespace Befriender.Core.Sources.Proximity.Scanners;

using Befriender.Core.Characters.Models;
using Befriender.Core.Sources.Proximity.Contracts;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin.Services;
using System.Collections.Generic;

public class ObjectTableProximityScanner : IProximityScanner {
    private IObjectTable objectTable;

    public ObjectTableProximityScanner(IObjectTable objectTable) {
        this.objectTable = objectTable;
    }

    public IEnumerable<Character> ScanNearbyPlayers() {
        var nearbyPlayers = new List<Character>();

        foreach (var obj in this.objectTable) {
            if (obj is IPlayerCharacter player && player.HomeWorld.RowId > 0 && !string.IsNullOrEmpty(player.Name.TextValue)) {
                nearbyPlayers.Add(new Character {
                    Name = player.Name.TextValue,
                    HomeWorldId = player.HomeWorld.RowId,
                    CurrentWorldId = player.CurrentWorld.RowId,
                    JobId = (byte)player.ClassJob.RowId,
                    Level = player.Level,
                    IsOnline = true
                });
            }
        }

        return nearbyPlayers;
    }
}