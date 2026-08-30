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

        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        Assert.Single(result);
        Assert.False(result[0].IsOnline);
        Assert.Equal(24, result[0].JobId);
        Assert.Equal(61510u, result[0].OnlineStateMask);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_OverwritesVolatileDataOnlyIfValid() {
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Existing Friend", IsOnline = true, JobId = 24, FcTag = "TEST", ClientLanguages = 2, GrandCompany = 0 }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        var scannedFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Existing Friend", IsOnline = false, JobId = 0, FcTag = string.Empty, ClientLanguages = 10, GrandCompany = 1 }
        };

        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        Assert.Single(result);
        Assert.False(result[0].IsOnline);
        Assert.Equal(24, result[0].JobId);
        Assert.Equal("TEST", result[0].FcTag);
        Assert.Equal(10, result[0].ClientLanguages);
        Assert.Equal(1, result[0].GrandCompany);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_IgnoresUpdatesIfCharacterIsNotLoggedIn() {
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns(string.Empty);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);
        var scannedFriends = new List<FriendProfile> { new FriendProfile { ContentId = 2, Name = "Ghost Friend" } };

        repository.UpdateFriends(scannedFriends);

        mockStorage.DidNotReceive().Save(Arg.Any<string>(), Arg.Any<IEnumerable<FriendProfile>>());
    }

    [Fact]
    public void FriendRepository_UpdateFriends_UpdatesLastSeenOnlyWhenOnline() {
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

        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        var friendA = result.First(f => f.ContentId == 1);
        var friendB = result.First(f => f.ContentId == 2);

        Assert.True(friendA.LastSeenAt > pastDate);
        Assert.Equal(pastDate, friendB.LastSeenAt);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_DetectsAndRecordsNameChanges() {
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

        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        Assert.Single(result);
        Assert.Equal("New Name", result[0].Name);
        Assert.NotNull(result[0].PreviousNames);
        Assert.Contains("Old Name", result[0].PreviousNames);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_FlagsDeletedCharacterIfNameIsEmpty() {
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

        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        Assert.Single(result);
        Assert.True(result[0].IsCharacterDeleted);
        Assert.Equal("Good Friend", result[0].Name);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_AutoArchivesFriendIfMissingFromScan() {
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

        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        Assert.Single(result);
        Assert.True(result[0].IsArchived);
        Assert.False(result[0].IsOnline);
    }

    [Fact]
    public void FriendRepository_Save_PersistsCurrentStateToStorage() {
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        repository.GetFriends();
        repository.Save();

        mockStorage.Received(1).Save("Almeris_33", Arg.Any<IEnumerable<FriendProfile>>());
    }

    [Fact]
    public void FriendRepository_ClearCache_EmptiesFriendsAndFiresEvent() {
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

        repository.ClearCache();

        mockIdentityService.GetCurrentCharacterId().Returns(string.Empty);
        var result = repository.GetFriends();

        Assert.Empty(result);
        Assert.True(eventFired);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_FiresFriendLoggedOnEventWhenFriendComesOnline() {
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Offline Friend", IsOnline = false }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        FriendProfile? notifiedFriend = null;
        repository.FriendLoggedOn += f => notifiedFriend = f;

        var scannedFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Offline Friend", IsOnline = true }
        };

        repository.UpdateFriends(scannedFriends);

        Assert.NotNull(notifiedFriend);
        Assert.Equal(1ul, notifiedFriend.ContentId);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_PreservesLocationWhenOfflineAndAcceptsZeroWhenOnline() {
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Offline Friend", IsOnline = false, LocationId = 123 },
            new FriendProfile { ContentId = 2, Name = "CrossWorld Friend", IsOnline = true, LocationId = 123 }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        var scannedFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Offline Friend", IsOnline = false, LocationId = 0 },
            new FriendProfile { ContentId = 2, Name = "CrossWorld Friend", IsOnline = true, LocationId = 0 }
        };

        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        var offlineFriend = result.First(f => f.ContentId == 1);
        var onlineFriend = result.First(f => f.ContentId == 2);

        Assert.Equal(123u, offlineFriend.LocationId);
        Assert.Equal(0u, onlineFriend.LocationId);
    }

    [Fact]
    public void FriendRepository_UpdateFriendFromCharacter_UpdatesCurrentWorldIdAndLocation() {
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Nearby Friend", CurrentWorldId = 99, LocationId = 50 }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var mockLocalPlayer = Substitute.For<Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>();
        mockObjectTable.LocalPlayer.Returns(mockLocalPlayer);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        var mockPlayer = Substitute.For<Dalamud.Game.ClientState.Objects.SubKinds.IPlayerCharacter>();
        mockPlayer.CompanyTag.Returns(new Dalamud.Game.Text.SeStringHandling.SeString(new Dalamud.Game.Text.SeStringHandling.Payloads.TextPayload("TAG")));

        // Le RowId du LocalPlayer par défaut sera 0 dans ce mock
        repository.UpdateFriendFromCharacter(1, mockPlayer, 123);
        var result = repository.GetFriends();

        var friend = result.First(f => f.ContentId == 1);
        Assert.Equal(0u, friend.CurrentWorldId);
        Assert.Equal(123u, friend.LocationId);
    }

    [Fact]
    public void EnsureLoaded_MigratesEmptyGuidsToNewGuids() {
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var legacyFriend = new FriendProfile { Id = Guid.Empty, ContentId = 1, Name = "Legacy Friend" };
        mockStorage.Load("Almeris_33").Returns(new List<FriendProfile> { legacyFriend });

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        // Accessing the collection triggers EnsureLoaded
        var friends = repository.GetFriends();

        Assert.NotEqual(Guid.Empty, friends[0].Id);
        mockStorage.Received(1).Save("Almeris_33", Arg.Any<IEnumerable<FriendProfile>>());
    }

    [Fact]
    public void GetCharacters_ProjectsFriendProfilesToCharactersWithCustomProperties() {
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var profileId = Guid.NewGuid();
        var friend = new FriendProfile { Id = profileId, ContentId = 1, Name = "Alice", IsArchived = true };
        mockStorage.Load("Almeris_33").Returns(new List<FriendProfile> { friend });

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState, mockObjectTable);

        var characters = repository.GetCharacters().ToList();

        Assert.Single(characters);
        var chara = characters[0];
        Assert.Equal(profileId, chara.Id);
        Assert.Equal("Alice", chara.Name);
        Assert.Equal("True", chara.CustomProperties["Befriender_IsArchived"]);
    }
}