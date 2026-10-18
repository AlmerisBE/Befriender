namespace Befriender.Tests.Core.Characters.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Characters.Services;
using Befriender.Core.GameData.Contracts;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class CharacterSearchServiceTests {
    private ICharacterTagRepository mockTagRepo;
    private ICharacterGroupRepository mockGroupRepo;
    private IGameDataService mockGameDataService;
    private CharacterSearchService searchService;

    public CharacterSearchServiceTests() {
        this.mockTagRepo = Substitute.For<ICharacterTagRepository>();
        this.mockGroupRepo = Substitute.For<ICharacterGroupRepository>();
        this.mockGameDataService = Substitute.For<IGameDataService>();

        this.searchService = new CharacterSearchService(this.mockTagRepo, this.mockGroupRepo, this.mockGameDataService);
    }

    [Fact]
    public void FilterCharacters_ReturnsMatches_AcrossAllConfiguredFields() {
        var savageTagId = Guid.NewGuid();
        var staticGroupId = Guid.NewGuid();

        this.mockTagRepo.GetTags().Returns(new List<CharacterTag> {
            new CharacterTag { Id = savageTagId, Name = "Savage" }
        });

        this.mockGroupRepo.GetGroups().Returns(new List<CharacterGroup> {
            new CharacterGroup { Id = staticGroupId, Title = "Static Group" }
        });

        this.mockGameDataService.GetLocationName(123u).Returns("Limsa Lominsa");
        this.mockGameDataService.GetDisplayLocation(456u, 0, 0, 0).Returns("Gridania");

        var characters = new List<Character> {
            new Character { Name = "John Doe" }, // Matches "John"
            new Character { Name = "Jane Smith", PreviousNames = new List<string> { "John Smith" } }, // Matches "John"
            new Character { Name = "Alice", FcTag = "JohnFC" }, // Matches "John"
            new Character { Name = "Bob", CustomGroupId = staticGroupId }, // Matches "Static"
            new Character { Name = "Charlie", Tags = new List<Guid> { savageTagId } }, // Matches "Savage"
            new Character { Name = "David", LocationId = 123u }, // Matches "Limsa"
            new Character { Name = "Eve" } // No match
        };

        var resultName = this.searchService.FilterCharacters(characters, "John");
        Assert.Equal(3, resultName.Count());

        var resultGroup = this.searchService.FilterCharacters(characters, "Static");
        Assert.Single(resultGroup);

        var resultTag = this.searchService.FilterCharacters(characters, "Savage");
        Assert.Single(resultTag);

        var resultLocation = this.searchService.FilterCharacters(characters, "Limsa");
        Assert.Single(resultLocation);
    }
}