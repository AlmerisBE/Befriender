namespace Befriender.Tests.UI.FriendList.Windows;

using Befriender.Core.Friends.Contracts;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.FriendList.Windows;
using NSubstitute;
using Xunit;

public class FriendListWindowTests {
    [Fact]
    public void FriendListWindow_Initialization_SetsCorrectPropertiesAndButtons() {
        // Arrange
        var mockRepo = Substitute.For<IFriendRepository>();
        var mockDisplay = Substitute.For<IFriendDisplayService>();
        var mockSync = Substitute.For<IFriendSyncService>();

        // Act
        var window = new FriendListWindow(mockRepo, mockDisplay, mockSync);

        // Assert
        Assert.Equal("Befriender - Friend List", window.WindowName);
        Assert.Single(window.TitleBarButtons);
    }
}