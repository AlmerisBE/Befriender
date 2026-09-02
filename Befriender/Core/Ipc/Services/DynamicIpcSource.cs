namespace Befriender.Core.Ipc.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using System;
using System.Collections.Generic;

public class DynamicIpcSource : ICharacterSource {
    public Guid SourceId { get; }
    public string Name { get; }
    public int Priority { get; }
    public bool IsSyncing { get; private set; }

    public event Action? DataUpdated;

    private List<Character> currentState = new();

    public DynamicIpcSource(Guid sourceId, string name, int priority) {
        this.SourceId = sourceId;
        this.Name = name;
        this.Priority = priority;
    }

    public IEnumerable<Character> GetCurrentState() {
        return this.currentState;
    }

    public void RequestManualRefresh() {
        // Since the data is pushed by an external plugin, we cannot force them to pull data here easily.
        // For a simple IPC implementation, we rely on the external plugin to push updates proactively.
    }

    public void UpdateState(IEnumerable<Character> characters) {
        this.currentState = new List<Character>(characters);
        this.DataUpdated?.Invoke();
    }
}