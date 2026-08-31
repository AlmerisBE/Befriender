namespace Befriender.Core.Sources.Proximity;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Sources.Proximity.Contracts;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class ProximitySource : ICharacterSource, IDisposable {
    private IProximityScanner scanner;
    private IFramework framework;
    private List<Character> currentState = new();

    private DateTime lastScanTime = DateTime.MinValue;
    private readonly TimeSpan scanInterval = TimeSpan.FromSeconds(2); // Scan every 2 seconds

    public Guid SourceId { get; } = Guid.Parse("S1000000-0000-0000-0000-000000000003");
    public string Name => "Proximity";
    public int Priority => 1; // Lowest priority, just used for "nearby" flags and current location updates

    public event Action? DataUpdated;

    public ProximitySource(IProximityScanner scanner, IFramework framework) {
        this.scanner = scanner;
        this.framework = framework;
        this.framework.Update += this.OnFrameworkUpdate;
    }

    public IEnumerable<Character> GetCurrentState() {
        return this.currentState.ToList();
    }

    private void OnFrameworkUpdate(IFramework fw) {
        var now = DateTime.Now;
        if (now - this.lastScanTime >= this.scanInterval) {
            this.lastScanTime = now;
            this.RefreshState();
        }
    }

    private void RefreshState() {
        this.currentState = this.scanner.ScanNearbyPlayers().ToList();
        this.DataUpdated?.Invoke();
    }

    public void Dispose() {
        this.framework.Update -= this.OnFrameworkUpdate;
    }
}