namespace Befriender.Tests.Core.Characters.Storage;

using Dalamud.Plugin;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Characters.Storage;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public class JsonCharacterGroupStorageTests : IDisposable {
    private IDalamudPluginInterface mockPluginInterface;
    private string tempDirectory;

    public JsonCharacterGroupStorageTests() {
        this.mockPluginInterface = Substitute.For<IDalamudPluginInterface>();

        this.tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.tempDirectory);

        this.mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(this.tempDirectory));
    }

    [Fact]
    public void SaveAndLoad_PersistsCharacterGroupsCorrectly() {
        var storage = new JsonCharacterGroupStorage(this.mockPluginInterface);
        string characterId = "TestAccount_123";
        var groupsToSave = new List<CharacterGroup> {
            new CharacterGroup { Id = Guid.NewGuid(), Title = "Raid Static", Description = "Savage group" }
        };

        storage.Save(characterId, groupsToSave);
        var loadedGroups = storage.Load(characterId).ToList();

        Assert.Single(loadedGroups);
        Assert.Equal(groupsToSave[0].Id, loadedGroups[0].Id);
        Assert.Equal("Raid Static", loadedGroups[0].Title);
        Assert.Equal("Savage group", loadedGroups[0].Description);
    }

    [Fact]
    public void Load_ReturnsEmptyList_WhenFileDoesNotExist() {
        var storage = new JsonCharacterGroupStorage(this.mockPluginInterface);

        var loadedGroups = storage.Load("UnknownAccount_000").ToList();

        Assert.Empty(loadedGroups);
    }

    [Fact]
    public void Load_ReturnsEmptyList_WhenFileIsCorrupted() {
        var storage = new JsonCharacterGroupStorage(this.mockPluginInterface);
        string characterId = "CorruptedAccount_123";
        string filePath = Path.Combine(this.tempDirectory, $"groups_{characterId}.json");

        File.WriteAllText(filePath, "{ Invalid JSON format }");

        var loadedGroups = storage.Load(characterId).ToList();

        Assert.Empty(loadedGroups);
    }

    public void Dispose() {
        if (Directory.Exists(this.tempDirectory)) Directory.Delete(this.tempDirectory, true);
    }
}