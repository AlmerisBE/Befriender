namespace Befriender.Tests.Core.Friends.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.Core.Friends.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class FriendRepositoryTests {
    [Fact]
    public void FriendRepository_UpdateFriends_PreservesMetadataForExistingFriends() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");
        mockClientState.TerritoryType.Returns((ushort)130);

        var originalDate = new DateTime(2023, 1, 1);
        var existingFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Old Friend", AddedAt = originalDate, AddedLocationId = 129 }
        };
        mockStorage.Load("Almeris_33").Returns(existingFriends);

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState);

        // Simuler un nouveau scan où l'ami est toujours là, mais sans métadonnées (le scanner n'a pas cette info)
        var scannedFriends = new List<FriendProfile> {
            new FriendProfile { ContentId = 1, Name = "Old Friend" }
        };

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        Assert.Single(result);
        Assert.Equal(originalDate, result[0].AddedAt);
        Assert.Equal(129, result[0].AddedLocationId);
    }

    [Fact]
    public void FriendRepository_UpdateFriends_AssignsMetadataToNewFriends() {
        // Arrange
        var mockStorage = Substitute.For<IFriendStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();
        var mockClientState = Substitute.For<IClientState>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");
        mockClientState.TerritoryType.Returns((ushort)130); // Zone actuelle du joueur
        mockStorage.Load("Almeris_33").Returns(new List<FriendProfile>());

        var repository = new FriendRepository(mockStorage, mockIdentityService, mockClientState);
        var scannedFriends = new List<FriendProfile> { new FriendProfile { ContentId = 2, Name = "New Friend" } };

        // Act
        repository.UpdateFriends(scannedFriends);
        var result = repository.GetFriends();

        // Assert
        Assert.Single(result);
        Assert.NotEqual(DateTime.MinValue, result[0].AddedAt); // Doit avoir été assigné à DateTime.Now
        Assert.Equal(130, result[0].AddedLocationId); // Doit correspondre à la zone actuelle
    }
}