namespace Befriender.Tests.UI.FriendList.Windows;

using Befriender.Core.Friends.Contracts;
using Befriender.UI.FriendList.Windows;
using Befriender.UI.Theme.Contracts;
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
        var mockTheme = Substitute.For<IThemeService>();

        // Act
        var window = new FriendListWindow(mockTabs, mockSync, mockTheme);

        // Assert
        Assert.Equal("Befriender", window.WindowName);
        Assert.Single(window.TitleBarButtons);
    }

    [Fact]
    public void FriendListWindow_Update_RequestsServerRefreshWhenWindowOpens() {
        // Arrange
        var mockTabs = new List<ITab>();
        var mockSync = Substitute.For<IFriendSyncService>();
        var mockTheme = Substitute.For<IThemeService>();

        // Assume the service thinks the window is closed initially
        mockSync.IsWindowOpen.Returns(false);

        var window = new FriendListWindow(mockTabs, mockSync, mockTheme);

        // Simulate the window being opened by Dalamud or a command
        window.IsOpen = true;

        // Act
        window.Update();

        // Assert
        mockSync.Received().IsWindowOpen = true;
        mockSync.Received(1).RequestServerRefresh();
    }
}