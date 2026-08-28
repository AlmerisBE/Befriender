namespace Befriender.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class JsonFriendGroupStorage : IFriendGroupStorage {
    private IDalamudPluginInterface pluginInterface;

    public JsonFriendGroupStorage(IDalamudPluginInterface pluginInterface) {
        this.pluginInterface = pluginInterface;
    }

    private string GetFilePath(string characterId) {
        return Path.Combine(this.pluginInterface.ConfigDirectory.FullName, $"groups_{characterId}.json");
    }

    public IReadOnlyList<FriendGroup> Load(string characterId) {
        if (string.IsNullOrEmpty(characterId)) {
            return new List<FriendGroup>();
        }

        var filePath = this.GetFilePath(characterId);
        if (!File.Exists(filePath)) {
            return new List<FriendGroup>();
        }

        try {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<FriendGroup>>(json) ?? new List<FriendGroup>();
        }
        catch {
            return new List<FriendGroup>();
        }
    }

    public void Save(string characterId, IEnumerable<FriendGroup> groups) {
        if (string.IsNullOrEmpty(characterId)) {
            return;
        }

        var filePath = this.GetFilePath(characterId);
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(groups, options);
        File.WriteAllText(filePath, json);
    }
}