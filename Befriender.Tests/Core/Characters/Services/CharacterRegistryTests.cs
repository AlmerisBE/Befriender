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
    private IFramework mockFramework;
    private IPluginLog mockPluginLog;

    public CharacterRegistryTests() {
        this.mockStorage = Substitute.For<ICharacterStorage>();
        this.mockMigration = Substitute.For<IMigrationService>();
        this.mockIdentity = Substitute.For<ICharacterIdentityService>();
        this.mockClientState = Substitute.For<IClientState>();
        this.mockFramework = Substitute.For<IFramework>();
        this.mockPluginLog = Substitute.For<IPluginLog>();
    }

    private CharacterRegistry CreateRegistry() {
        return new CharacterRegistry(
            this.mockStorage,
            Array.Empty<ICharacterSource>(),
            this.mockMigration,
            this.mockIdentity,
            this.mockClientState,
            this.mockFramework,
            this.mockPluginLog);
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

    [Fact]
    public void OnFrameworkUpdate_InitializesRegistryWhenPlayerIsAvailable() {
        var registry = this.CreateRegistry();

        this.mockClientState.IsLoggedIn.Returns(true);
        this.mockIdentity.GetCurrentCharacterId().Returns("TestAccount_33");

        this.mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(this.mockFramework);

        this.mockMigration.Received(1).RunMigrations("TestAccount_33");
        this.mockStorage.Received(1).Load("MasterCharacterList", "TestAccount_33");
    }

    [Fact]
    public void RequestManualRefresh_InvokesRefreshOnAllRegisteredSources() {
        var registry = this.CreateRegistry();
        var mockSource = Substitute.For<ICharacterSource>();
        mockSource.SourceId.Returns(Guid.NewGuid());

        registry.RegisterSource(mockSource);
        registry.RequestManualRefresh();

        mockSource.Received(1).RequestManualRefresh();
    }

    [Fact]
    public void ProcessSourceUpdate_DoesNotOverwriteValidLocationWithZero() {
        var registry = this.CreateRegistry();
        var mockSource = Substitute.For<ICharacterSource>();
        mockSource.SourceId.Returns(Guid.NewGuid());

        var initialChar = new Character { ContentId = 1, Name = "Alice", HomeWorldId = 33, LocationId = 129 };
        mockSource.GetCurrentState().Returns(new List<Character> { initialChar });
        registry.RegisterSource(mockSource);
        mockSource.DataUpdated += Raise.Event<Action>();

        var incomingChar = new Character { ContentId = 1, Name = "Alice", HomeWorldId = 33, LocationId = 0 };
        mockSource.GetCurrentState().Returns(new List<Character> { incomingChar });
        mockSource.DataUpdated += Raise.Event<Action>();

        var allChars = registry.GetAllCharacters();
        Assert.Single(allChars);
        Assert.Equal(129u, allChars[0].LocationId);
    }

    [Fact]
    public void ProcessSourceUpdate_RetainsSourceId_WhenIncomingCharacterLacksContentId() {
        // Arrange
        var registry = this.CreateRegistry();

        var mockPrimarySource = Substitute.For<ICharacterSource>();
        mockPrimarySource.SourceId.Returns(Guid.NewGuid());

        var mockProximitySource = Substitute.For<ICharacterSource>();
        var proximityGuid = Guid.Parse("51000000-0000-0000-0000-000000000003");
        mockProximitySource.SourceId.Returns(proximityGuid);

        var primaryChar = new Character { ContentId = 12345, Name = "Alice", HomeWorldId = 33 };
        mockPrimarySource.GetCurrentState().Returns(new List<Character> { primaryChar });
        registry.RegisterSource(mockPrimarySource);
        mockPrimarySource.DataUpdated += Raise.Event<Action>();

        registry.RegisterSource(mockProximitySource);

        // Act
        var proxChar = new Character { ContentId = 0, Name = "Alice", HomeWorldId = 33 };
        mockProximitySource.GetCurrentState().Returns(new List<Character> { proxChar });
        mockProximitySource.DataUpdated += Raise.Event<Action>();

        // Assert
        var allChars = registry.GetAllCharacters();
        Assert.Single(allChars);

        var resolvedChar = allChars[0];
        Assert.Equal(12345ul, resolvedChar.ContentId);

        Assert.Contains(proximityGuid, resolvedChar.ActiveSourceIds);
    }
}