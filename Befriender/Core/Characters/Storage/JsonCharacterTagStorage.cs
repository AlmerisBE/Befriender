namespace Befriender.Core.Characters.Storage;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class JsonCharacterTagStorage : ICharacterTagStorage {
    private IDalamudPluginInterface pluginInterface;
    private IPluginLog pluginLog;
    private JsonSerializerOptions jsonOptions;

    public JsonCharacterTagStorage(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog) {
        this.pluginInterface = pluginInterface;
        this.pluginLog = pluginLog;
        this.jsonOptions = new JsonSerializerOptions {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        };
    }

    private string GetFilePath(string characterId) {
        return Path.Combine(this.pluginInterface.ConfigDirectory.FullName, $"tags_{characterId}.json");
    }

    public IEnumerable<CharacterTag> Load(string characterId) {
        if (string.IsNullOrEmpty(characterId)) {
            return new List<CharacterTag>();
        }

        string path = this.GetFilePath(characterId);
        if (!File.Exists(path)) {
            return new List<CharacterTag>();
        }

        try {
            string json = File.ReadAllText(path);
            var tags = JsonSerializer.Deserialize<List<CharacterTag>>(json, this.jsonOptions);

            if (tags != null) {
                return tags;
            }
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, $"Failed to load character tags from disk for character {characterId}");
        }

        return new List<CharacterTag>();
    }

    public void Save(string characterId, IEnumerable<CharacterTag> tags) {
        if (string.IsNullOrEmpty(characterId)) {
            return;
        }

        string path = this.GetFilePath(characterId);

        try {
            string json = JsonSerializer.Serialize(tags, this.jsonOptions);
            File.WriteAllText(path, json);
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, $"Failed to save character tags to disk for character {characterId}");
        }
    }
}