namespace Befriender.Core.Friends.Storage;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class JsonFriendTagStorage : IFriendTagStorage {
    private IDalamudPluginInterface pluginInterface;
    private IPluginLog pluginLog;

    public JsonFriendTagStorage(IDalamudPluginInterface pluginInterface, IPluginLog pluginLog) {
        this.pluginInterface = pluginInterface;
        this.pluginLog = pluginLog;
    }

    private string GetFilePath(string characterId) {
        return Path.Combine(this.pluginInterface.ConfigDirectory.FullName, $"tags_{characterId}.json");
    }

    public IEnumerable<FriendTag> Load(string characterId) {
        if (string.IsNullOrEmpty(characterId)) {
            return new List<FriendTag>();
        }

        string path = this.GetFilePath(characterId);
        if (!File.Exists(path)) {
            return new List<FriendTag>();
        }

        try {
            string json = File.ReadAllText(path);
            var tags = JsonSerializer.Deserialize<List<FriendTag>>(json);

            if (tags != null) {
                return tags;
            }
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, $"Failed to load friend tags from disk for character {characterId}");
        }

        return new List<FriendTag>();
    }

    public void Save(string characterId, IEnumerable<FriendTag> tags) {
        if (string.IsNullOrEmpty(characterId)) {
            return;
        }

        string path = this.GetFilePath(characterId);

        try {
            string json = JsonSerializer.Serialize(tags, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, $"Failed to save friend tags to disk for character {characterId}");
        }
    }
}