namespace Befriender.Tests.Core.Characters.Services;

using Dalamud.Plugin.Services;
using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Characters.Services;
using global::Befriender.Core.Migrations.Contracts;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class CharacterRegistryTests {
    private ICharacterStorage mockStorage;
    private IMigrationService mockMigration;
    private ICharacterIdentityService mockIdentity;
    private IClientState mockClientState;

    public CharacterRegistryTests() {
        this.mockStorage = Substitute.For<ICharacterStorage>();
        this.mockMigration = Substitute.For<IMigrationService>();
        this.mockIdentity = Substitute.For<ICharacterIdentityService>();
        this.mockClientState = Substitute.For<IClientState>();
    }

    private CharacterRegistry CreateRegistry() {
        return new CharacterRegistry(this.mockStorage, Array.Empty<ICharacterSource>(), this.mockMigration, this.mockIdentity, this.mockClientState);
    }

    [Fact]
    public void ProcessSourceUpdate_AddsNewCharacterAndLinksSourceId() {
        var registry = this.CreateRegistry();
        var mockSource = Substitute.For<ICharacterSource>();
        var sourceId = Guid.NewGuid();
        mockSource.SourceId.Returns(sourceId);

        var incomingChar = new Character { ContentId = 1, Name = "Alice", HomeWorldId = 33 };
        mockSource.GetCurrentState().Returns(new List<Character> { incomingChar });

        registry.RegisterSource(mockSource);
        mockSource.DataUpdated += Raise.Event<Action>();

        var allChars = registry.GetAllCharacters();
        Assert.Single(allChars);
        Assert.Contains(sourceId, allChars[0].ActiveSourceIds);
        Assert.True(allChars[0].IsActivelyTracked);
    }

    [Fact]
    public void ProcessSourceUpdate_RemovesSourceIdFromMissingCharacters() {
        var registry = this.CreateRegistry();
        registry.LoadMasterList("TestAccount");

        var mockSource = Substitute.For<ICharacterSource>();
        var sourceId = Guid.NewGuid();
        mockSource.SourceId.Returns(sourceId);

        var incomingChar = new Character { ContentId = 1, Name = "Alice", HomeWorldId = 33 };
        mockSource.GetCurrentState().Returns(new List<Character> { incomingChar });

        registry.RegisterSource(mockSource);
        mockSource.DataUpdated += Raise.Event<Action>();

        mockSource.GetCurrentState().Returns(new List<Character>());
        mockSource.DataUpdated += Raise.Event<Action>();

        var allChars = registry.GetAllCharacters();
        Assert.Single(allChars);
        Assert.Empty(allChars[0].ActiveSourceIds);
        Assert.False(allChars[0].IsActivelyTracked);
    }
}