namespace Befriender.Tests.UI.FriendList.Windows;

using Befriender.Core.Friends.Contracts;
using Befriender.UI.FriendList.Windows;
using NSubstitute;
using Xunit;

public class FriendListWindowTests {
    [Fact]
    public void FriendListWindow_Initialization_SetsCorrectProperties() {
        // Arrange
        var mockRepo = Substitute.For<IFriendRepository>();

        // Act
        var window = new FriendListWindow(mockRepo);

        // Assert
        Assert.Equal("Befriender - Friend List", window.WindowName);
    }
}