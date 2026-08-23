namespace Befriender.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class JsonFriendStorage : IFriendStorage {
    private string filePath;

    public JsonFriendStorage(IDalamudPluginInterface pluginInterface) {
        this.filePath = Path.Combine(pluginInterface.ConfigDirectory.FullName, "friends.json");
    }

    public IReadOnlyList<FriendProfile> Load() {
        if (!File.Exists(this.filePath)) {
            return new List<FriendProfile>();
        }

        try {
            var json = File.ReadAllText(this.filePath);
            return JsonSerializer.Deserialize<List<FriendProfile>>(json) ?? new List<FriendProfile>();
        }
        catch {
            return new List<FriendProfile>();
        }
    }

    public void Save(IEnumerable<FriendProfile> friends) {
        var options = new JsonSerializerOptions { WriteIndented = true };
        var json = JsonSerializer.Serialize(friends, options);
        File.WriteAllText(this.filePath, json);
    }
}