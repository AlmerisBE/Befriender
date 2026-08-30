namespace Befriender.Tests.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Characters.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class CharacterRegistryTests {
    [Fact]
    public void RegisterSource_ConsolidatesCharactersFromMultipleSources() {
        var registry = new CharacterRegistry();

        var source1 = Substitute.For<ICharacterSource>();
        source1.SourceId.Returns("Archive");
        source1.Priority.Returns(10);
        source1.IsEnabled.Returns(true);
        source1.GetCharacters().Returns(new List<Character> {
            new Character { ContentId = 1, Name = "Alice", HomeWorldId = 33, IsOnline = false }
        });

        var source2 = Substitute.For<ICharacterSource>();
        source2.SourceId.Returns("FriendList");
        source2.Priority.Returns(20);
        source2.IsEnabled.Returns(true);
        source2.GetCharacters().Returns(new List<Character> {
            new Character { ContentId = 1, Name = "Alice", HomeWorldId = 33, IsOnline = true, Level = 90 }
        });

        registry.RegisterSource(source1);
        registry.RegisterSource(source2);

        var result = registry.GetConsolidatedCharacters();

        Assert.Single(result);
        var alice = result[0];
        Assert.Equal("Alice", alice.Name);
        Assert.True(alice.IsOnline); // Source 2 priority wins
        Assert.Equal(90, alice.Level);
        Assert.Contains("Archive", alice.ActiveSources);
        Assert.Contains("FriendList", alice.ActiveSources);
    }

    [Fact]
    public void ConsolidateData_IgnoresDisabledSources() {
        var registry = new CharacterRegistry();

        var source1 = Substitute.For<ICharacterSource>();
        source1.SourceId.Returns("Party");
        source1.Priority.Returns(50);
        source1.IsEnabled.Returns(false); // Disabled
        source1.GetCharacters().Returns(new List<Character> {
            new Character { ContentId = 2, Name = "Bob", HomeWorldId = 34 }
        });

        registry.RegisterSource(source1);

        var result = registry.GetConsolidatedCharacters();

        Assert.Empty(result);
    }
}