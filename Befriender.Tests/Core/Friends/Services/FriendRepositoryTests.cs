namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Friends.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class FriendRepositoryTests {
    [Fact]
    public void FriendRepository_UpdateFriends_UpsertsDataWithoutRemovingMissingFriends() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Persisted Friend", IsOnline = true, JobId = 24 }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState);

        // Simulating an empty scan (e.g., during character switch or vanilla list opening)
        var scannedFriends = new List<FriendProfile>();

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        Assert.Single(result); // The friend is NOT removed
        Assert.False(result[0].IsOnline); // But they are marked offline
        Assert.Equal(24, result[0].JobId); // And their last known job is preserved
    }

    [Fact]
    public void FriendRepository_UpdateFriends_OverwritesVolatileDataOnlyIfValid() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Existing Friend", IsOnline = true, JobId = 24, FcTag = "TEST" }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState);

        // Scan returns the same friend, but offline and with cleared game data
        var scannedFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Existing Friend", IsOnline = false, JobId = 0, FcTag = string.Empty }
        };

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].IsOnline);
        Assert.Equal(24, result[0].JobId); // Should not be overwritten by 0
        Assert.Equal("TEST", result[0].FcTag); // Should not be overwritten by empty
    }

    [Fact]
    public void FriendRepository_UpdateFriends_IgnoresUpdatesIfCharacterIsNotLoggedIn() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();

        // Simulating logout or loading screen where character is undetermined
        mockIdentityService.GetCurrentCharacterId().Returns(string.Empty);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState);
        var scannedFriends = new List<FriendProfile> { new FriendProfile { ContentId = 2, Name = "Ghost Friend" } };

        // Act
        repository.UpdateFriends(scannedFriends);

        // Assert
        mockStorage.DidNotReceive().Save(Arg.Any<string>(), Arg.Any<IEnumerable<FriendProfile>>());
    }

    [Fact]
    public void FriendRepository_UpdateFriends_UpdatesLastSeenOnlyWhenOnline() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var pastDate = DateTime.Now.AddDays(-2);
        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Friend A", IsOnline = false, LastSeenAt = pastDate },
            new FriendProfile { ContentId = 2, Name = "Friend B", IsOnline = false, LastSeenAt = pastDate }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState);

        // Friend A comes online, Friend B stays offline
        var scannedFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Friend A", IsOnline = true }
        };

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        var friendA = result.First(f => f.ContentId == 1);
        var friendB = result.First(f => f.ContentId == 2);

        Assert.True(friendA.LastSeenAt > pastDate); // Updated to ~now
        Assert.Equal(pastDate, friendB.LastSeenAt); // Preserved
    }
}