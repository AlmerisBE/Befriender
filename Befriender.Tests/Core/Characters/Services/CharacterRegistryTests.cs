namespace Befriender.Tests.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Characters.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class CharacterRegistryTests {
    [Fact]
    public void ProcessSourceUpdate_AddsNewCharacterAndLinksSourceId() {
        var mockStorage = Substitute.For<ICharacterStorage>();
        var registry = new CharacterRegistry(mockStorage, Array.Empty<ICharacterSource>());

        var mockSource = Substitute.For<ICharacterSource>();
        var sourceId = Guid.NewGuid();
        mockSource.SourceId.Returns(sourceId);

        var incomingChar = new Character { ContentId = 1, Name = "Alice", HomeWorldId = 33 };
        mockSource.GetCurrentState().Returns(new List<Character> { incomingChar });

        registry.RegisterSource(mockSource);

        // Simulate DataUpdated event
        mockSource.DataUpdated += Raise.Event<Action>();

        var allChars = registry.GetAllCharacters();
        Assert.Single(allChars);
        Assert.Contains(sourceId, allChars[0].ActiveSourceIds);
        Assert.True(allChars[0].IsActivelyTracked);
    }

    [Fact]
    public void ProcessSourceUpdate_RemovesSourceIdFromMissingCharacters() {
        var mockStorage = Substitute.For<ICharacterStorage>();
        var registry = new CharacterRegistry(mockStorage, Array.Empty<ICharacterSource>());
        registry.LoadMasterList("TestAccount");

        var mockSource = Substitute.For<ICharacterSource>();
        var sourceId = Guid.NewGuid();
        mockSource.SourceId.Returns(sourceId);

        // First pass: Alice is in the source
        var incomingChar = new Character { ContentId = 1, Name = "Alice", HomeWorldId = 33 };
        mockSource.GetCurrentState().Returns(new List<Character> { incomingChar });

        registry.RegisterSource(mockSource);
        mockSource.DataUpdated += Raise.Event<Action>();

        // Second pass: Alice is no longer in the source (e.g. removed from friends)
        mockSource.GetCurrentState().Returns(new List<Character>());
        mockSource.DataUpdated += Raise.Event<Action>();

        var allChars = registry.GetAllCharacters();
        Assert.Single(allChars); // Alice remains in the Master List (Archived state)
        Assert.Empty(allChars[0].ActiveSourceIds); // But she is no longer tracked by any source
        Assert.False(allChars[0].IsActivelyTracked);
    }
}