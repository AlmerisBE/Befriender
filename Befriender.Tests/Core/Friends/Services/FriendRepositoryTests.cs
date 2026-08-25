namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Friends.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class FriendRepositoryTests {
    [Fact]
    public void FriendRepository_UpdateFriends_UpsertsDataWithoutRemovingMissingFriends() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Persisted Friend", IsOnline = true, JobId = 24, OnlineStateMask = 61510 }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        var scannedFriends = new List<FriendProfile>();

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].IsOnline);
        Assert.Equal(24, result[0].JobId);
        Assert.Equal(61510u, result[0].OnlineStateMask);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_OverwritesVolatileDataOnlyIfValid() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Existing Friend", IsOnline = true, JobId = 24, FcTag = "TEST", ClientLanguages = 2 }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        var scannedFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Existing Friend", IsOnline = false, JobId = 0, FcTag = string.Empty, ClientLanguages = 10 }
        };

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        Assert.Single(result);
        Assert.False(result[0].IsOnline);
        Assert.Equal(24, result[0].JobId);
        Assert.Equal("TEST", result[0].FcTag);
        Assert.Equal(10, result[0].ClientLanguages);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_IgnoresUpdatesIfCharacterIsNotLoggedIn() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns(string.Empty);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);
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
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var pastDate = DateTime.Now.AddDays(-2);
        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Friend A", IsOnline = false, LastSeenAt = pastDate },
            new FriendProfile { ContentId = 2, Name = "Friend B", IsOnline = false, LastSeenAt = pastDate }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        var scannedFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Friend A", IsOnline = true }
        };

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        var friendA = result.First(f => f.ContentId == 1);
        var friendB = result.First(f => f.ContentId == 2);

        Assert.True(friendA.LastSeenAt > pastDate);
        Assert.Equal(pastDate, friendB.LastSeenAt);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_DetectsAndRecordsNameChanges() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 999, Name = "Old Name", HomeWorldId = 33 }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        var scannedFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 999, Name = "New Name", HomeWorldId = 33, IsOnline = true }
        };

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        Assert.Single(result);
        Assert.Equal("New Name", result[0].Name);
        Assert.NotNull(result[0].PreviousNames);
        Assert.Contains("Old Name", result[0].PreviousNames);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_FlagsDeletedCharacterIfNameIsEmpty() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Good Friend" }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        var scannedFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = string.Empty }
        };

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        Assert.Single(result);
        Assert.True(result[0].IsCharacterDeleted);
        Assert.Equal("Good Friend", result[0].Name);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_AutoArchivesFriendIfMissingFromScan() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Removed Friend", IsArchived = false }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        var scannedFriends = new List<FriendProfile>();

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        Assert.Single(result);
        Assert.True(result[0].IsArchived);
        Assert.False(result[0].IsOnline);
    }

    [Fact]
    public void FriendRepository_Save_PersistsCurrentStateToStorage() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        // Act
        // We simulate reading data first, to initialize the internal loadedCharacterId
        repository.GetFriends();
        repository.Save();

        // Assert
        mockStorage.Received(1).Save("Almeris_33", Arg.Any<IEnumerable<FriendProfile>>());
    }

    [Fact]
    public void FriendRepository_ClearCache_EmptiesFriendsAndFiresEvent() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");
        mockStorage.Load("Almeris_33").Returns(new List<FriendProfile> { new FriendProfile { ContentId = 1 } });

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);
        repository.GetFriends();

        bool eventFired = false;
        repository.CacheCleared += () => eventFired = true;

        // Act
        repository.ClearCache();

        mockIdentityService.GetCurrentCharacterId().Returns(string.Empty);
        var result = repository.GetFriends();

        // Assert
        Assert.Empty(result);
        Assert.True(eventFired);
    }
}