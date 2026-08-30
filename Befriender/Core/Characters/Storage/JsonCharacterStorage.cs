namespace Befriender.Core.Characters.Storage;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class JsonCharacterStorage : ICharacterStorage {
    private IDalamudPluginInterface pluginInterface;
    private IPluginLog pluginLog;
    private JsonSerializerOptions jsonOptions;

    public JsonCharacterStorage(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog) {
        this.pluginInterface = pluginInterface;
        this.pluginLog = pluginLog;
        this.jsonOptions = new JsonSerializerOptions { WriteIndented = true };
    }

    private string GetFilePath(string storeName, string accountIdentity) {
        return Path.Combine(this.pluginInterface.ConfigDirectory.FullName, $"{storeName}_{accountIdentity}.json");
    }

    public IEnumerable<Character> Load(string storeName, string accountIdentity) {
        if (string.IsNullOrEmpty(storeName) || string.IsNullOrEmpty(accountIdentity)) {
            return new List<Character>();
        }

        string filePath = this.GetFilePath(storeName, accountIdentity);
        if (!File.Exists(filePath)) {
            return new List<Character>();
        }

        try {
            string json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<Character>>(json, this.jsonOptions) ?? new List<Character>();
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, $"Failed to load character storage for {storeName}_{accountIdentity}");
            return new List<Character>();
        }
    }

    public void Save(string storeName, string accountIdentity, IEnumerable<Character> characters) {
        if (string.IsNullOrEmpty(storeName) || string.IsNullOrEmpty(accountIdentity)) {
            return;
        }

        string filePath = this.GetFilePath(storeName, accountIdentity);

        try {
            var directory = Path.GetDirectoryName(filePath);
            if (directory != null && !Directory.Exists(directory)) {
                Directory.CreateDirectory(directory);
            }

            string json = JsonSerializer.Serialize(characters, this.jsonOptions);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, $"Failed to save character storage for {storeName}_{accountIdentity}");
        }
    }
}