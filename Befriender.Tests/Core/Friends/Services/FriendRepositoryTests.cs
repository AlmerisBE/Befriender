namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Friends.Services;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class FriendRepositoryTests {
    [Fact]
    public void FriendRepository_GetFriends_LoadsFromStorageWhenCharacterChanges() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var dummyFriends = new List<FriendProfile> { new FriendProfile { Name = "Persisted Friend" } };
        mockStorage.Load("Almeris_33").Returns(dummyFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService);

        // Act
        var friends = repository.GetFriends();

        // Assert
        Assert.Single(friends);
        Assert.Equal("Persisted Friend", friends[0].Name);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_SavesToStorageWithCorrectCharacterId() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");
        mockStorage.Load("Almeris_33").Returns(new List<FriendProfile>());

        var repository = new FriendRepository(mockStorage, mockIdentityService);
        var dummyFriends = new List<FriendProfile> { new FriendProfile { Name = "New Friend" } };

        // Act
        repository.UpdateFriends(dummyFriends);

        // Assert
        mockStorage.Received(1).Save("Almeris_33", Arg.Is<IEnumerable<FriendProfile>>(list => list.Count() == 1 && list.First().Name == "New Friend"));
    }
}