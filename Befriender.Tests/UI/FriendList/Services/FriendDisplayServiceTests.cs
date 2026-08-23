namespace Befriender.Tests.UI.FriendList.Services;

using Befriender.Core.Friends.Models;
using Befriender.UI.FriendList.Services;
using System.Collections.Generic;
using Xunit;

public class FriendDisplayServiceTests {
    private List<FriendProfile> dummyFriends = new() {
        new FriendProfile { Name = "Zebra Offline", IsOnline = false, FcTag = "ZOO" },
        new FriendProfile { Name = "Alpha Online", IsOnline = true, FcTag = "ABC" },
        new FriendProfile { Name = "Charlie Online", IsOnline = true, FcTag = "" }
    };

    [Fact]
    public void FriendDisplayService_ProcessFriends_FiltersOnlineOnly() {
        // Arrange
        var service = new FriendDisplayService();

        // Act
        var result = service.ProcessFriends(this.dummyFriends, true, -1, true);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.All(result, f => Assert.True(f.IsOnline));
    }

    [Fact]
    public void FriendDisplayService_ProcessFriends_SortsByNameAscending() {
        // Arrange
        var service = new FriendDisplayService();

        // Act
        // Column 1 is Name
        var result = service.ProcessFriends(this.dummyFriends, false, 1, true);

        // Assert
        Assert.Equal("Alpha Online", result[0].Name);
        Assert.Equal("Charlie Online", result[1].Name);
        Assert.Equal("Zebra Offline", result[2].Name);
    }

    [Fact]
    public void FriendDisplayService_ProcessFriends_SortsByStatusDescending() {
        // Arrange
        var service = new FriendDisplayService();

        // Act
        // Column 0 is Status (Descending usually puts True/Online first)
        var result = service.ProcessFriends(this.dummyFriends, false, 0, false);

        // Assert
        Assert.True(result[0].IsOnline);
        Assert.True(result[1].IsOnline);
        Assert.False(result[2].IsOnline);
    }
}