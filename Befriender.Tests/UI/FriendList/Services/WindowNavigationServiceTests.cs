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
    public void ToggleProfilePanel_FiresOnProfilePanelToggledEvent() {
        var service = new WindowNavigationService();
        bool? toggledState = null;
        service.OnProfilePanelToggled += state => toggledState = state;

        service.ToggleProfilePanel(true);

        Assert.True(toggledState);
    }
}