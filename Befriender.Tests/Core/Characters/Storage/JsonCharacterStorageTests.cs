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

public class JsonCharacterStorageTests : IDisposable {
    private IDalamudPluginInterface mockPluginInterface;
    private IPluginLog mockPluginLog;
    private string tempDirectory;

    public JsonCharacterStorageTests() {
        this.mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        this.mockPluginLog = Substitute.For<IPluginLog>();

        this.tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.tempDirectory);

        this.mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(this.tempDirectory));
    }

    [Fact]
    public void SaveAndLoad_PersistsCharacterDataCorrectly() {
        var storage = new JsonCharacterStorage(this.mockPluginInterface, this.mockPluginLog);
        var sourceId = Guid.NewGuid();

        var character = new Character {
            Id = Guid.NewGuid(),
            Name = "John Doe",
            ContentId = 123456789,
            HomeWorldId = 33,
            IsOnline = true,
            SourceSpecificData = new Dictionary<Guid, Dictionary<string, string>> {
                { sourceId, new Dictionary<string, string> { { "TestKey", "TestValue" } } }
            }
        };

        var charactersToSave = new List<Character> { character };
        string accountIdentity = "TestUser_33";

        // Act
        storage.Save("MasterCharacterList", accountIdentity, charactersToSave);
        var loadedCharacters = storage.Load("MasterCharacterList", accountIdentity).ToList();

        // Assert
        Assert.Single(loadedCharacters);
        var loaded = loadedCharacters[0];

        Assert.Equal(character.Id, loaded.Id);
        Assert.Equal(character.Name, loaded.Name);
        Assert.True(loaded.SourceSpecificData.ContainsKey(sourceId));
        Assert.Equal("TestValue", loaded.SourceSpecificData[sourceId]["TestKey"]);
    }

    [Fact]
    public void Load_ReturnsEmptyList_WhenFileDoesNotExist() {
        var storage = new JsonCharacterStorage(this.mockPluginInterface, this.mockPluginLog);

        var loadedCharacters = storage.Load("MasterCharacterList", "UnknownUser_00").ToList();

        Assert.Empty(loadedCharacters);
    }

    public void Dispose() {
        if (Directory.Exists(this.tempDirectory)) {
            Directory.Delete(this.tempDirectory, true);
        }
    }
}