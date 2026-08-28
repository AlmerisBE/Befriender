namespace Befriender.Core.Friends.Storage;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class JsonFriendStorage : IFriendStorage {
    private IDalamudPluginInterface pluginInterface;

    public JsonFriendStorage(IDalamudPluginInterface pluginInterface) {
        this.pluginInterface = pluginInterface;
    }

    private string GetFilePath(string characterId) {
        return Path.Combine(this.pluginInterface.ConfigDirectory.FullName, $"friends_{characterId}.json");
    }

    public IReadOnlyList<FriendProfile> Load(string characterId) {
        if (string.IsNullOrEmpty(characterId)) {
            return new List<FriendProfile>();
        }

        var filePath = this.GetFilePath(characterId);
        if (!File.Exists(filePath)) {
            return new List<FriendProfile>();
        }

        try {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<FriendProfile>>(json) ?? new List<FriendProfile>();
        }
        catch {
            return new List<FriendProfile>();
        }
    }

    public void Save(string characterId, IEnumerable<FriendProfile> friends) {
        if (string.IsNullOrEmpty(characterId)) {
            return;
        }

        var filePath = this.GetFilePath(characterId);
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(friends, options);
        File.WriteAllText(filePath, json);
    }
}