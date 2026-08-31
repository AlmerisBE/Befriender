namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Friends.Services;
using Befriender.Core.Migrations.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class FriendRepositoryTests {
    [Fact]
    public void FriendRepository_UpdateFriends_UpsertsDataWithoutRemovingMissingFriends() {
        var mockStorage = Substitute.For<ICharacterStorage>();
        var mockMigration = Substitute.For<IMigrationService>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        // The properties are now strongly typed on the Character model
        var char1 = new Character {
            ContentId = 1,
            Name = "Persisted Friend",
            IsOnline = true,
            JobId = 24,
            OnlineStateMask = 61510u
        };

        mockStorage.Load("FriendList", "Almeris_33").Returns(new List<Character> { char1 });

        var repository = new FriendRepository(mockStorage, mockMigration, mockIdentityService, mockClientState, mockObjectTable);

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
        var mockStorage = Substitute.For<ICharacterStorage>();
        var mockMigration = Substitute.For<IMigrationService>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var char1 = new Character {
            ContentId = 1,
            Name = "Existing Friend",
            IsOnline = true,
            JobId = 24,
            FcTag = "TEST",
            ClientLanguages = 2,
            GrandCompany = 0
        };

        mockStorage.Load("FriendList", "Almeris_33").Returns(new List<Character> { char1 });

        var repository = new FriendRepository(mockStorage, mockMigration, mockIdentityService, mockClientState, mockObjectTable);

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
}