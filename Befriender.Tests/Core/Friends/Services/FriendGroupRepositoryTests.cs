namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Friends.Services;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class FriendGroupRepositoryTests {
    [Fact]
    public void GetGroups_SeedsVanillaGroups_WhenStorageIsEmpty() {
        var mockStorage = Substitute.For<IFriendGroupStorage>();
        var mockIdentity = Substitute.For<ICharacterIdentityService>();

        mockIdentity.GetCurrentCharacterId().Returns("Almeris_33");
        mockStorage.Load("Almeris_33").Returns(new List<FriendGroup>());

        var repository = new FriendGroupRepository(mockStorage, mockIdentity);
        var groups = repository.GetGroups();

        Assert.Equal(8, groups.Count);
        Assert.Equal(0, groups[0].Id);
        Assert.Equal(7, groups[7].Id);
    }

    [Fact]
    public void UpdateGroup_UpdatesMetadataAndSaves_WhenGroupExists() {
        var mockStorage = Substitute.For<IFriendGroupStorage>();
        var mockIdentity = Substitute.For<ICharacterIdentityService>();

        mockIdentity.GetCurrentCharacterId().Returns("Almeris_33");

        var existingGroups = new List<FriendGroup> {
            new FriendGroup { Id = 1, Title = "Old Title" }
        };
        mockStorage.Load("Almeris_33").Returns(existingGroups);

        var repository = new FriendGroupRepository(mockStorage, mockIdentity);
        repository.UpdateGroup(new FriendGroup { Id = 1, Title = "New Title", Description = "Test" });

        var groups = repository.GetGroups();
        var updatedGroup = groups.First(g => g.Id == 1);

        Assert.Equal("New Title", updatedGroup.Title);
        Assert.Equal("Test", updatedGroup.Description);
        mockStorage.Received(1).Save("Almeris_33", Arg.Any<IEnumerable<FriendGroup>>());
    }
}