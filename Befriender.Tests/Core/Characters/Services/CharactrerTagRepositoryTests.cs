namespace Befriender.Tests.Core.Characters.Services;

using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Characters.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class CharacterTagRepositoryTests {
    private ICharacterTagStorage mockStorage;
    private ICharacterIdentityService mockIdentity;

    public CharacterTagRepositoryTests() {
        this.mockStorage = Substitute.For<ICharacterTagStorage>();
        this.mockIdentity = Substitute.For<ICharacterIdentityService>();
        this.mockIdentity.GetCurrentCharacterId().Returns("TestUser_33");
    }

    [Fact]
    public void AddTag_CreatesNewTagAndSaves() {
        var repo = new CharacterTagRepository(this.mockStorage, this.mockIdentity);

        repo.AddTag("NewTag");

        var tags = repo.GetTags();
        Assert.Single(tags);
        Assert.Equal("NewTag", tags[0].Name);
        this.mockStorage.Received(1).Save("TestUser_33", Arg.Any<IEnumerable<CharacterTag>>());
    }

    [Fact]
    public void AddTag_PreventsDuplicates() {
        var repo = new CharacterTagRepository(this.mockStorage, this.mockIdentity);
        var existingTags = new List<CharacterTag> { new CharacterTag { Name = "ExistingTag" } };
        this.mockStorage.Load("TestUser_33").Returns(existingTags);

        repo.AddTag("existingtag"); // Différente casse

        Assert.Single(repo.GetTags()); // Toujours un seul élément
        this.mockStorage.DidNotReceive().Save(Arg.Any<string>(), Arg.Any<IEnumerable<CharacterTag>>());
    }
}