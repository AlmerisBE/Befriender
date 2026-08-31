namespace Befriender.Core.Characters.Storage;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

public class JsonCharacterGroupStorage : ICharacterGroupStorage {
    private IDalamudPluginInterface pluginInterface;
    private JsonSerializerOptions jsonOptions;

    public JsonCharacterGroupStorage(IDalamudPluginInterface pluginInterface) {
        this.pluginInterface = pluginInterface;
        this.jsonOptions = new JsonSerializerOptions {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true,
            IncludeFields = true
        };
    }

    private string GetFilePath(string characterId) {
        return Path.Combine(this.pluginInterface.ConfigDirectory.FullName, $"groups_{characterId}.json");
    }

    public IReadOnlyList<CharacterGroup> Load(string characterId) {
        if (string.IsNullOrEmpty(characterId)) {
            return new List<CharacterGroup>();
        }

        var filePath = this.GetFilePath(characterId);
        if (!File.Exists(filePath)) {
            return new List<CharacterGroup>();
        }

        try {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<List<CharacterGroup>>(json, this.jsonOptions) ?? new List<CharacterGroup>();
        }
        catch {
            return new List<CharacterGroup>();
        }
    }

    public void Save(string characterId, IEnumerable<CharacterGroup> groups) {
        if (string.IsNullOrEmpty(characterId)) {
            return;
        }

        var filePath = this.GetFilePath(characterId);
        var json = JsonSerializer.Serialize(groups, this.jsonOptions);
        File.WriteAllText(filePath, json);
    }
}