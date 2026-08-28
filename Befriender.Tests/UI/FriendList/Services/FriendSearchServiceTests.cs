namespace Befriender.Tests.UI.FriendList.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using Befriender.UI.FriendList.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class FriendSearchServiceTests {
    [Fact]
    public void FilterFriends_EmptyQuery_ReturnsAllFriends() {
        var mockTagRepo = Substitute.For<IFriendTagRepository>();
        var friends = new List<FriendProfile> { new FriendProfile { Name = "Alice" }, new FriendProfile { Name = "Bob" } };
        var service = new FriendSearchService(mockTagRepo);

        var result = service.FilterFriends(friends, "");

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public void FilterFriends_MatchesNameCaseInsensitive() {
        var mockTagRepo = Substitute.For<IFriendTagRepository>();
        var friends = new List<FriendProfile> {
            new FriendProfile { Name = "Alice Liddell" },
            new FriendProfile { Name = "Bob Builder" }
        };
        var service = new FriendSearchService(mockTagRepo);

        var result = service.FilterFriends(friends, "alice");

        Assert.Single(result);
        Assert.Equal("Alice Liddell", result[0].Name);
    }

    [Fact]
    public void FilterFriends_MatchesTagCaseInsensitive() {
        var tagId = Guid.NewGuid();
        var mockTagRepo = Substitute.For<IFriendTagRepository>();
        mockTagRepo.GetTags().Returns(new List<FriendTag> { new FriendTag { Id = tagId, Name = "Raider" } });

        var friends = new List<FriendProfile> {
            new FriendProfile { Name = "Alice", Tags = new List<Guid> { tagId } },
            new FriendProfile { Name = "Bob", Tags = new List<Guid>() }
        };

        var service = new FriendSearchService(mockTagRepo);
        var result = service.FilterFriends(friends, "RAID");

        Assert.Single(result);
        Assert.Equal("Alice", result[0].Name);
    }
}