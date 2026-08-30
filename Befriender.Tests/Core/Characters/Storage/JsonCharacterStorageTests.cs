namespace Befriender.Tests.Core.Characters.Storage;

using Befriender.Core.Characters.Models;
using Befriender.Core.Characters.Storage;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public class JsonCharacterStorageTests : IDisposable {
    private List<string> createdDirectories = new();

    private string GetUniqueTempPath() {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        this.createdDirectories.Add(path);
        return path;
    }

    [Fact]
    public void SaveAndLoad_PersistsCharactersWithCustomPropertiesAndSourceIds() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockPluginLog = Substitute.For<IPluginLog>();

        string fakePath = this.GetUniqueTempPath();
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(fakePath));

        var storage = new JsonCharacterStorage(mockPluginInterface, mockPluginLog);
        var sourceId = Guid.NewGuid();
        var characterId = Guid.NewGuid();

        var character = new Character {
            Id = characterId,
            ContentId = 12345,
            Name = "Storage Test",
            HomeWorldId = 33,
            IsOnline = true
        };
        character.ActiveSourceIds.Add(sourceId);
        character.CustomProperties["TestKey"] = "TestValue";

        var charactersToSave = new List<Character> { character };

        storage.Save("TestStore", "Account1", charactersToSave);
        var loadedCharacters = storage.Load("TestStore", "Account1").ToList();

        Assert.Single(loadedCharacters);
        var loaded = loadedCharacters[0];
        Assert.Equal(characterId, loaded.Id);
        Assert.Equal((ulong)12345, loaded.ContentId);
        Assert.Equal("Storage Test", loaded.Name);
        Assert.Equal((uint)33, loaded.HomeWorldId);
        Assert.True(loaded.IsOnline);
        Assert.Contains(sourceId, loaded.ActiveSourceIds);
        Assert.True(loaded.CustomProperties.ContainsKey("TestKey"));
        Assert.Equal("TestValue", loaded.CustomProperties["TestKey"]);
    }

    [Fact]
    public void Load_ReturnsEmptyListWhenFileDoesNotExist() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockPluginLog = Substitute.For<IPluginLog>();

        string fakePath = this.GetUniqueTempPath();
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(fakePath));

        var storage = new JsonCharacterStorage(mockPluginInterface, mockPluginLog);

        var loadedCharacters = storage.Load("NonExistentStore", "Account1").ToList();

        Assert.Empty(loadedCharacters);

        // Fix: Explicitly declare argument types to resolve CS0121 ambiguity
        mockPluginLog.DidNotReceive().Error(Arg.Any<Exception>(), Arg.Any<string>());
    }

    public void Dispose() {
        foreach (var dir in this.createdDirectories) {
            if (Directory.Exists(dir)) {
                try {
                    Directory.Delete(dir, true);
                }
                catch { }
            }
        }
    }
}