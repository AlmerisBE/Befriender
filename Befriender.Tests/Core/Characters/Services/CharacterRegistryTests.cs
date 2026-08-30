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
    public void RegisterSource_ConsolidatesCharactersAndCustomProperties() {
        // Fix: Inject empty array to satisfy constructor parameters
        var registry = new CharacterRegistry(Array.Empty<ICharacterSource>());

        var sourceId1 = Guid.NewGuid();
        var source1 = Substitute.For<ICharacterSource>();
        source1.SourceId.Returns(sourceId1);
        source1.Priority.Returns(10);
        source1.IsEnabled.Returns(true);

        var char1 = new Character { ContentId = 1, Name = "Alice", HomeWorldId = 33, IsOnline = false };
        char1.CustomProperties["ExtPlugin_Rank"] = "Gold";
        source1.GetCharacters().Returns(new List<Character> { char1 });

        var sourceId2 = Guid.NewGuid();
        var source2 = Substitute.For<ICharacterSource>();
        source2.SourceId.Returns(sourceId2);
        source2.Priority.Returns(20);
        source2.IsEnabled.Returns(true);

        var char2 = new Character { ContentId = 1, Name = "Alice", HomeWorldId = 33, IsOnline = true, Level = 90 };
        char2.CustomProperties["Another_Data"] = "Test";
        char2.CustomProperties["ExtPlugin_Rank"] = "Platinum";
        source2.GetCharacters().Returns(new List<Character> { char2 });

        registry.RegisterSource(source1);
        registry.RegisterSource(source2);

        var result = registry.GetConsolidatedCharacters();

        Assert.Single(result);
        var alice = result[0];
        Assert.Equal("Alice", alice.Name);
        Assert.True(alice.IsOnline);
        Assert.Equal(90, alice.Level);
        Assert.Contains(sourceId1, alice.ActiveSourceIds);
        Assert.Contains(sourceId2, alice.ActiveSourceIds);

        Assert.Equal("Test", alice.CustomProperties["Another_Data"]);
        Assert.Equal("Platinum", alice.CustomProperties["ExtPlugin_Rank"]);
    }
}