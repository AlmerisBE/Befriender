namespace Befriender.Tests.UI.FriendList.Windows;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.FreeCompany.Contracts;
using Befriender.Core.Friends.Contracts;
using Befriender.Core.Input.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.FriendList.Components;
using Befriender.UI.FriendList.Contracts;
using Befriender.UI.FriendList.Windows;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Windows.Contracts;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class FriendListWindowTests {
    private FriendListWindow CreateSystemUnderTests(
        out IFriendSyncService mockSync,
        out IFreeCompanySyncService mockFcSync,
        out IWindowNavigationService mockNav,
        out IHotkeyService mockHotkey) {

        var tabs = new List<ITab>();
        mockSync = Substitute.For<IFriendSyncService>();
        mockFcSync = Substitute.For<IFreeCompanySyncService>();
        var mockTheme = Substitute.For<IThemeService>();
        mockHotkey = Substitute.For<IHotkeyService>();
        mockNav = Substitute.For<IWindowNavigationService>();
        var mockConfig = Substitute.For<IConfigurationService>();

        mockConfig.GetConfig().Returns(new PluginConfiguration());

        // Instantiate the concrete StatusBar component with its required mocks
        var mockLoc = Substitute.For<ILocalizationService>();
        var mockFriendRepo = Substitute.For<IFriendRepository>();
        var statusBarComponent = new FriendStatusBarComponent(mockSync, mockLoc, mockFriendRepo);

        return new FriendListWindow(tabs, mockSync, mockFcSync, mockTheme, mockHotkey, mockNav, mockConfig, statusBarComponent);
    }

    [Fact]
    public void OnOpen_StartsSyncServicesAndRequestsRefresh() {
        var window = this.CreateSystemUnderTests(out var mockSync, out var mockFcSync, out _, out _);

        window.OnOpen();

        mockSync.Received(1).IsWindowOpen = true;
        mockSync.Received(1).RequestServerRefresh();
        mockFcSync.Received(1).StartSync();
    }

    [Fact]
    public void OnClose_StopsSyncServices() {
        var window = this.CreateSystemUnderTests(out var mockSync, out var mockFcSync, out _, out _);

        window.OnClose();

        mockSync.Received(1).IsWindowOpen = false;
        mockFcSync.Received(1).StopSync();
    }

    [Fact]
    public void Dispose_UnregistersEvents() {
        var window = this.CreateSystemUnderTests(out _, out _, out var mockNav, out var mockHotkey);

        window.Dispose();

        mockHotkey.Received(1).OnHotkeyPressed -= Arg.Any<Action>();
        mockNav.Received(1).OnWindowToggleRequested -= Arg.Any<Action>();
        mockNav.Received(1).OnTabRequested -= Arg.Any<Action<string>>();
    }
}