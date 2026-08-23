namespace Befriender.Tests.UI.FriendList.Windows;

using Befriender.UI.FriendList.Windows;
using Xunit;

public class FriendListWindowTests {
    [Fact]
    public void FriendListWindow_Initialization_SetsCorrectProperties() {
        // Arrange & Act
        var window = new FriendListWindow();

        // Assert
        Assert.Equal("Befriender - Friend List", window.WindowName);
    }
}