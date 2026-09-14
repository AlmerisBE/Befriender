namespace Befriender.Tests.UI.MainWindow.Components;

using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Localization.Contracts;
using global::Befriender.UI.MainWindow.Components;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class TagManagementComponentTests {
    private ICharacterTagRepository mockTagRepository;
    private ICharacterRegistry mockRegistry;
    private ILocalizationService mockLoc;

    public TagManagementComponentTests() {
        this.mockTagRepository = Substitute.For<ICharacterTagRepository>();
        this.mockRegistry = Substitute.For<ICharacterRegistry>();
        this.mockLoc = Substitute.For<ILocalizationService>();
    }

    [Fact]
    public void CreateTag_CallsRepository_WhenNameIsValid() {
        var component = new TagManagementComponent(this.mockTagRepository, this.mockRegistry, this.mockLoc);

        component.CreateTag("New Tag");

        this.mockTagRepository.Received(1).AddTag("New Tag");
    }

    [Fact]
    public void DeleteTag_RemovesTagsFromCharactersAndSaves_BeforeRemovingTag() {
        var component = new TagManagementComponent(this.mockTagRepository, this.mockRegistry, this.mockLoc);
        var tagId = Guid.NewGuid();

        var char1 = new Character { Name = "Alice", Tags = new List<Guid> { tagId } };
        var char2 = new Character { Name = "Bob", Tags = new List<Guid> { Guid.NewGuid() } };
        this.mockRegistry.GetAllCharacters().Returns(new List<Character> { char1, char2 });

        component.DeleteTag(tagId);

        Assert.Empty(char1.Tags);
        Assert.Single(char2.Tags); // Unaffected character
        this.mockRegistry.Received(1).SaveMasterList();
        this.mockTagRepository.Received(1).RemoveTag(tagId);
    }
}