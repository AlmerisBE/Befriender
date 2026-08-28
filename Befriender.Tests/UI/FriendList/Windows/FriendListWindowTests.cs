namespace Befriender.Tests.UI.FriendList.Windows;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Input.Contracts;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.FriendList.Windows;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Windows.Contracts;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class FriendListWindowTests {
    [Fact]
    public void FriendListWindow_Initialization_SetsCorrectPropertiesAndButtons() {
        var mockTabs = new List<ITab>();
        var mockSync = Substitute.For<IFriendSyncService>();
        var mockTheme = Substitute.For<IThemeService>();
        var mockHotkey = Substitute.For<IHotkeyService>();
        var mockNavService = Substitute.For<IWindowNavigationService>();
        var mockConfigService = Substitute.For<IConfigurationService>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { IsProfilePanelOpen = false });

        var window = new FriendListWindow(mockTabs, mockSync, mockTheme, mockHotkey, mockNavService, mockConfigService);

        Assert.Equal("Befriender", window.WindowName);
        Assert.Single(window.TitleBarButtons);
    }

    [Fact]
    public void FriendListWindow_OnOpen_RequestsServerAndCrossWorldRefresh() {
        var mockTabs = new List<ITab>();
        var mockSync = Substitute.For<IFriendSyncService>();
        var mockTheme = Substitute.For<IThemeService>();
        var mockHotkey = Substitute.For<IHotkeyService>();
        var mockNavService = Substitute.For<IWindowNavigationService>();
        var mockConfigService = Substitute.For<IConfigurationService>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { IsProfilePanelOpen = false });

        var window = new FriendListWindow(mockTabs, mockSync, mockTheme, mockHotkey, mockNavService, mockConfigService);

        window.OnOpen();

        mockSync.Received().IsWindowOpen = true;
        mockSync.Received(1).RequestServerRefresh();
    }

    [Fact]
    public void FriendListWindow_OnClose_UpdatesSyncServiceState() {
        var mockTabs = new List<ITab>();
        var mockSync = Substitute.For<IFriendSyncService>();
        var mockTheme = Substitute.For<IThemeService>();
        var mockHotkey = Substitute.For<IHotkeyService>();
        var mockNavService = Substitute.For<IWindowNavigationService>();
        var mockConfigService = Substitute.For<IConfigurationService>();

        mockConfigService.GetConfig().Returns(new PluginConfiguration { IsProfilePanelOpen = false });

        var window = new FriendListWindow(mockTabs, mockSync, mockTheme, mockHotkey, mockNavService, mockConfigService);

        window.OnClose();

        mockSync.Received().IsWindowOpen = false;
    }
}