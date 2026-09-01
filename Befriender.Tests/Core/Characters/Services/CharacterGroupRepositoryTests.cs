namespace Befriender.Tests.Core.Characters.Services;

using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Characters.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class CharacterGroupRepositoryTests {
    private ICharacterGroupStorage mockStorage;
    private ICharacterIdentityService mockIdentity;

    public CharacterGroupRepositoryTests() {
        this.mockStorage = Substitute.For<ICharacterGroupStorage>();
        this.mockIdentity = Substitute.For<ICharacterIdentityService>();
        this.mockIdentity.GetCurrentCharacterId().Returns("TestUser_33");
    }

    [Fact]
    public void AddGroup_CreatesNewGroupAndSaves() {
        var repo = new CharacterGroupRepository(this.mockStorage, this.mockIdentity);

        repo.AddGroup("New Group");

        var groups = repo.GetGroups();
        Assert.Single(groups);
        Assert.Equal("New Group", groups[0].Title);
        this.mockStorage.Received(1).Save("TestUser_33", Arg.Any<IEnumerable<CharacterGroup>>());
    }

    [Fact]
    public void RemoveGroup_DeletesGroupAndSaves() {
        var repo = new CharacterGroupRepository(this.mockStorage, this.mockIdentity);
        var groupId = Guid.NewGuid();
        var existingGroups = new List<CharacterGroup> { new CharacterGroup { Id = groupId, Title = "To Delete" } };
        this.mockStorage.Load("TestUser_33").Returns(existingGroups);

        repo.RemoveGroup(groupId);

        Assert.Empty(repo.GetGroups());
        this.mockStorage.Received(1).Save("TestUser_33", Arg.Any<IEnumerable<CharacterGroup>>());
    }
}