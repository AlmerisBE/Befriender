namespace Befriender.Tests.UI.FriendList.Windows;

using Befriender.Core.Friends.Contracts;
using Befriender.UI.FriendList.Windows;
using Befriender.UI.Windows.Contracts;
using NSubstitute;
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
}