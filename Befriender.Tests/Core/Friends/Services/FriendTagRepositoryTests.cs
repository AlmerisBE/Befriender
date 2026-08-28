namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Friends.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class FriendTagRepositoryTests {
    [Fact]
    public void AddTag_TruncatesNameTo30CharactersAndSaves() {
        var mockStorage = Substitute.For<IFriendTagStorage>();
        var mockIdentity = Substitute.For<ICharacterIdentityService>();

        mockIdentity.GetCurrentCharacterId().Returns("Almeris_33");
        mockStorage.Load("Almeris_33").Returns(new List<FriendTag>());

        var repository = new FriendTagRepository(mockStorage, mockIdentity);

        string longName = "ThisIsAVeryLongTagNameThatExceedsThirtyCharactersForTesting";
        repository.AddTag(longName);

        var tags = repository.GetTags();
        Assert.Single(tags);
        Assert.Equal("ThisIsAVeryLongTagNameThatExce", tags[0].Name); // Exactly 30 characters
        Assert.Equal(30, tags[0].Name.Length);
        mockStorage.Received(1).Save("Almeris_33", Arg.Any<IEnumerable<FriendTag>>());
    }

    [Fact]
    public void AddTag_IgnoresEmptyOrWhitespaceStrings() {
        var mockStorage = Substitute.For<IFriendTagStorage>();
        var mockIdentity = Substitute.For<ICharacterIdentityService>();

        mockIdentity.GetCurrentCharacterId().Returns("Almeris_33");
        mockStorage.Load("Almeris_33").Returns(new List<FriendTag>());

        var repository = new FriendTagRepository(mockStorage, mockIdentity);

        repository.AddTag("");
        repository.AddTag("   ");

        var tags = repository.GetTags();
        Assert.Empty(tags);
        mockStorage.DidNotReceive().Save(Arg.Any<string>(), Arg.Any<IEnumerable<FriendTag>>());
    }

    [Fact]
    public void AddTag_PreventsDuplicateTagNamesCaseInsensitive() {
        var mockStorage = Substitute.For<IFriendTagStorage>();
        var mockIdentity = Substitute.For<ICharacterIdentityService>();

        var existingTags = new List<FriendTag> { new FriendTag { Name = "RaidStatic" } };

        mockIdentity.GetCurrentCharacterId().Returns("Almeris_33");
        mockStorage.Load("Almeris_33").Returns(existingTags);

        var repository = new FriendTagRepository(mockStorage, mockIdentity);

        repository.AddTag("raidstatic"); // Should be ignored

        var tags = repository.GetTags();
        Assert.Single(tags);
        mockStorage.DidNotReceive().Save(Arg.Any<string>(), Arg.Any<IEnumerable<FriendTag>>());
    }
}