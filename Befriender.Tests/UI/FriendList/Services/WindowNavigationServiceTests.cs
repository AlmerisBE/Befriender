namespace Befriender.Tests.UI.FriendList.Services;

using Befriender.UI.FriendList.Services;
using Xunit;

public class WindowNavigationServiceTests {
    [Fact]
    public void OpenTab_FiresOnTabRequestedEvent() {
        var service = new WindowNavigationService();
        string? requestedTab = null;
        service.OnTabRequested += tab => requestedTab = tab;

        service.OpenTab("Tab_Config");

        Assert.Equal("Tab_Config", requestedTab);
    }

    [Fact]
    public void ToggleWindow_FiresOnWindowToggleRequestedEvent() {
        var service = new WindowNavigationService();
        bool eventFired = false;
        service.OnWindowToggleRequested += () => eventFired = true;

        service.ToggleWindow();

        Assert.True(eventFired);
    }
}