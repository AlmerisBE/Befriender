namespace Befriender.Core.Ipc.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Ipc.Contracts;
using Befriender.Core.Ipc.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

public class IpcProviderService : IIpcProviderService, IDisposable {
    private IDalamudPluginInterface pluginInterface;
    private ICharacterRegistry registry;
    private IPluginLog pluginLog;

    private ICallGateProvider<string>? getCharactersProvider;
    private ICallGateProvider<string, string, int, bool>? registerSourceProvider;
    private ICallGateProvider<string, string, bool>? updateSourceProvider;

    private Dictionary<string, DynamicIpcSource> externalSources = new();
    private JsonSerializerOptions jsonOptions;

    public IpcProviderService(IDalamudPluginInterface pluginInterface, ICharacterRegistry registry, IPluginLog pluginLog) {
        this.pluginInterface = pluginInterface;
        this.registry = registry;
        this.pluginLog = pluginLog;

        this.jsonOptions = new JsonSerializerOptions {
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

    public void Initialize() {
        this.getCharactersProvider = this.pluginInterface.GetIpcProvider<string>("Befriender.GetCharacters");
        this.getCharactersProvider.RegisterFunc(this.GetCharactersJson);

        this.registerSourceProvider = this.pluginInterface.GetIpcProvider<string, string, int, bool>("Befriender.RegisterSource");
        this.registerSourceProvider.RegisterFunc(this.RegisterExternalSource);

        this.updateSourceProvider = this.pluginInterface.GetIpcProvider<string, string, bool>("Befriender.UpdateSourceData");
        this.updateSourceProvider.RegisterFunc(this.UpdateExternalSourceData);
    }

    private string GetCharactersJson() {
        var dtos = this.registry.GetAllCharacters().Select(c => new IpcCharacterDto {
            ContentId = c.ContentId,
            Name = c.Name,
            HomeWorldId = c.HomeWorldId,
            CurrentWorldId = c.CurrentWorldId,
            LocationId = c.LocationId,
            JobId = c.JobId,
            IsOnline = c.IsOnline,
            FcTag = c.FcTag
        }).ToList();

        return JsonSerializer.Serialize(dtos, this.jsonOptions);
    }

    private bool RegisterExternalSource(string sourceGuidStr, string sourceName, int priority) {
        if (!Guid.TryParse(sourceGuidStr, out var sourceId)) {
            return false;
        }

        if (this.externalSources.ContainsKey(sourceGuidStr)) {
            return true;
        }

        var source = new DynamicIpcSource(sourceId, sourceName, priority);
        this.externalSources[sourceGuidStr] = source;
        this.registry.RegisterSource(source);

        this.pluginLog.Debug($"[IPC] External source registered: {sourceName} ({sourceId})");
        return true;
    }

    private bool UpdateExternalSourceData(string sourceGuidStr, string jsonData) {
        if (!this.externalSources.TryGetValue(sourceGuidStr, out var source)) {
            return false;
        }

        try {
            var dtos = JsonSerializer.Deserialize<List<IpcCharacterDto>>(jsonData, this.jsonOptions);
            if (dtos == null) {
                return false;
            }

            var characters = dtos.Select(dto => new Character {
                ContentId = dto.ContentId,
                Name = dto.Name,
                HomeWorldId = dto.HomeWorldId,
                CurrentWorldId = dto.CurrentWorldId,
                LocationId = dto.LocationId,
                JobId = dto.JobId,
                IsOnline = dto.IsOnline,
                FcTag = dto.FcTag
            }).ToList();

            source.UpdateState(characters);
            return true;
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, $"[IPC] Failed to parse data update for external source {sourceGuidStr}");
            return false;
        }
    }

    public void Dispose() {
        this.getCharactersProvider?.UnregisterFunc();
        this.registerSourceProvider?.UnregisterFunc();
        this.updateSourceProvider?.UnregisterFunc();
    }
}