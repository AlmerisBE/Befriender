namespace Befriender.Core.Sources.Proximity.Contracts;

using Befriender.Core.Characters.Models;
using System.Collections.Generic;

public interface IProximityScanner {
    IEnumerable<Character> ScanNearbyPlayers();
}