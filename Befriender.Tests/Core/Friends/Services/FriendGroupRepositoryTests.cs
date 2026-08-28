namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Friends.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class FriendGroupRepositoryTests {
    [Fact]
    public void AddGroup_CreatesNewGroupAndSaves() {
        var mockStorage = Substitute.For<IFriendGroupStorage>();
        var mockIdentity = Substitute.For<ICharacterIdentityService>();

        mockIdentity.GetCurrentCharacterId().Returns("Almeris_33");
        mockStorage.Load("Almeris_33").Returns(new List<FriendGroup>());

        var repository = new FriendGroupRepository(mockStorage, mockIdentity);

        repository.AddGroup("My Custom Group");
        var groups = repository.GetGroups();

        Assert.Single(groups);
        Assert.Equal("My Custom Group", groups[0].Title);
        mockStorage.Received(1).Save("Almeris_33", Arg.Any<IEnumerable<FriendGroup>>());
    }

    [Fact]
    public void RemoveGroup_DeletesGroupAndSaves() {
        var mockStorage = Substitute.For<IFriendGroupStorage>();
        var mockIdentity = Substitute.For<ICharacterIdentityService>();

        var groupId = Guid.NewGuid();
        var existingGroups = new List<FriendGroup> { new FriendGroup { Id = groupId, Title = "To Delete" } };

        mockIdentity.GetCurrentCharacterId().Returns("Almeris_33");
        mockStorage.Load("Almeris_33").Returns(existingGroups);

        var repository = new FriendGroupRepository(mockStorage, mockIdentity);

        repository.RemoveGroup(groupId);
        var groups = repository.GetGroups();

        Assert.Empty(groups);
        mockStorage.Received(1).Save("Almeris_33", Arg.Any<IEnumerable<FriendGroup>>());
    }

    [Fact]
    public void MoveGroupUp_SwapsWithPreviousElementAndSaves() {
        var mockStorage = Substitute.For<IFriendGroupStorage>();
        var mockIdentity = Substitute.For<ICharacterIdentityService>();

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var existingGroups = new List<FriendGroup> {
            new FriendGroup { Id = id1, Title = "Group 1" },
            new FriendGroup { Id = id2, Title = "Group 2" }
        };

        mockIdentity.GetCurrentCharacterId().Returns("Almeris_33");
        mockStorage.Load("Almeris_33").Returns(existingGroups);

        var repository = new FriendGroupRepository(mockStorage, mockIdentity);

        repository.MoveGroupUp(id2);
        var groups = repository.GetGroups();

        Assert.Equal(id2, groups[0].Id);
        Assert.Equal(id1, groups[1].Id);
        mockStorage.Received(1).Save("Almeris_33", Arg.Any<IEnumerable<FriendGroup>>());
    }

    [Fact]
    public void MoveGroupDown_SwapsWithNextElementAndSaves() {
        var mockStorage = Substitute.For<IFriendGroupStorage>();
        var mockIdentity = Substitute.For<ICharacterIdentityService>();

        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var existingGroups = new List<FriendGroup> {
            new FriendGroup { Id = id1, Title = "Group 1" },
            new FriendGroup { Id = id2, Title = "Group 2" }
        };

        mockIdentity.GetCurrentCharacterId().Returns("Almeris_33");
        mockStorage.Load("Almeris_33").Returns(existingGroups);

        var repository = new FriendGroupRepository(mockStorage, mockIdentity);

        repository.MoveGroupDown(id1);
        var groups = repository.GetGroups();

        Assert.Equal(id2, groups[0].Id);
        Assert.Equal(id1, groups[1].Id);
        mockStorage.Received(1).Save("Almeris_33", Arg.Any<IEnumerable<FriendGroup>>());
    }
}