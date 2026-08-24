namespace Befriender.Tests.UI.FriendList.Windows;

using Befriender.Core.Friends.Contracts;
using Befriender.UI.FriendList.Windows;
using Befriender.UI.Windows.Contracts;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class FriendListWindowTests {
    [Fact]
    public void FriendListWindow_Initialization_SetsCorrectPropertiesAndButtons() {
        // Arrange
        var mockTabs = new List<ITab>();
        var mockSync = Substitute.For<IFriendSyncService>();

        // Act
        var window = new FriendListWindow(mockTabs, mockSync);

        // Assert
        Assert.Equal("Befriender", window.WindowName);
        Assert.Single(window.TitleBarButtons);
    }

    [Fact]
    public void FriendListWindow_Update_RequestsServerRefreshWhenWindowOpens() {
        // Arrange
        var mockTabs = new List<ITab>();
        var mockSync = Substitute.For<IFriendSyncService>();

        // Assume the service thinks the window is closed initially
        mockSync.IsWindowOpen.Returns(false);

        var window = new FriendListWindow(mockTabs, mockSync);

        // Simulate the window being opened by Dalamud or a command
        window.IsOpen = true;

        // Act
        window.Update();

        // Assert
        // We verify that the property was updated and the server refresh was requested
        mockSync.Received().IsWindowOpen = true;
        mockSync.Received(1).RequestServerRefresh();
    }
}