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
    public void FriendRepository_Initialization_LoadsFromStorage() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var dummyFriends = new List<FriendProfile> { new FriendProfile { Name = "Persisted Friend" } };
        mockStorage.Load().Returns(dummyFriends);

        // Act
        var repository = new FriendRepository(mockStorage);
        var friends = repository.GetFriends();

        // Assert
        Assert.Single(friends);
        Assert.Equal("Persisted Friend", friends[0].Name);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_SavesToStorage() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        mockStorage.Load().Returns(new List<FriendProfile>());
        var repository = new FriendRepository(mockStorage);
        var dummyFriends = new List<FriendProfile> { new FriendProfile { Name = "New Friend" } };

        // Act
        repository.UpdateFriends(dummyFriends);

        // Assert
        // We verify the contents of the list instead of the reference, because the repository creates a new list instance.
        mockStorage.Received(1).Save(Arg.Is<IEnumerable<FriendProfile>>(list => list.Count() == 1 && list.First().Name == "New Friend"));
    }
}