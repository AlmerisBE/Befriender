namespace Befriender.Tests.Core.Characters.Storage;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Characters.Storage;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public class JsonCharacterTagStorageTests : IDisposable {
    private IDalamudPluginInterface mockPluginInterface;
    private IPluginLog mockPluginLog;
    private string tempDirectory;

    public JsonCharacterTagStorageTests() {
        this.mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        this.mockPluginLog = Substitute.For<IPluginLog>();

        this.tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.tempDirectory);

        this.mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(this.tempDirectory));
    }

    [Fact]
    public void SaveAndLoad_PersistsCharacterTagsCorrectly() {
        var storage = new JsonCharacterTagStorage(this.mockPluginInterface, this.mockPluginLog);
        string characterId = "TestAccount_123";
        var tagsToSave = new List<CharacterTag> {
            new CharacterTag { Id = Guid.NewGuid(), Name = "Crafter" }
        };

        storage.Save(characterId, tagsToSave);
        var loadedTags = storage.Load(characterId).ToList();

        Assert.Single(loadedTags);
        Assert.Equal(tagsToSave[0].Id, loadedTags[0].Id);
        Assert.Equal("Crafter", loadedTags[0].Name);
    }

    [Fact]
    public void Load_ReturnsEmptyList_WhenFileDoesNotExist() {
        var storage = new JsonCharacterTagStorage(this.mockPluginInterface, this.mockPluginLog);

        var loadedTags = storage.Load("UnknownAccount_000").ToList();

        Assert.Empty(loadedTags);
    }

    [Fact]
    public void Load_ReturnsEmptyList_WhenFileIsCorrupted() {
        var storage = new JsonCharacterTagStorage(this.mockPluginInterface, this.mockPluginLog);
        string characterId = "CorruptedAccount_123";
        string filePath = Path.Combine(this.tempDirectory, $"tags_{characterId}.json");

        File.WriteAllText(filePath, "{ Invalid JSON format }");

        var loadedTags = storage.Load(characterId).ToList();

        Assert.Empty(loadedTags);
        this.mockPluginLog.Received(1).Error(Arg.Any<Exception>(), Arg.Is<string>(s => s.Contains("Failed to load character tags from disk")));
    }

    public void Dispose() {
        if (Directory.Exists(this.tempDirectory)) Directory.Delete(this.tempDirectory, true);
    }
}