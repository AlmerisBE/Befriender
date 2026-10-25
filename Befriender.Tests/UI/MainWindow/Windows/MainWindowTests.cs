namespace Befriender.Tests.UI.MainWindow.Windows;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.Input.Contracts;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.MainWindow.Contracts;
using Befriender.UI.MainWindow.Windows;
using Befriender.UI.Theme.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class MainWindowTests {
    private IClientState mockClientState;
    private MainWindow mainWindow;

    public MainWindowTests() {
        this.mockClientState = Substitute.For<IClientState>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        mockConfigService.GetConfig().Returns(new PluginConfiguration());

        this.mainWindow = new MainWindow(
            new List<ITab>(),
            Substitute.For<ICharacterRegistry>(),
            mockConfigService,
            Substitute.For<ILocalizationService>(),
            Substitute.For<IWindowNavigationService>(),
            Substitute.For<FriendStatusBarComponent>(Substitute.For<ICharacterRegistry>(), new List<ICharacterSource>(), Substitute.For<ILocalizationService>()),
            Substitute.For<RemoveConfirmationModalComponent>(Substitute.For<IRemoveCharacterRequestService>(), Substitute.For<ICharacterRegistry>(), Substitute.For<ILocalizationService>()),
            Substitute.For<IHotkeyService>(),
            Substitute.For<IThemeService>(),
            Substitute.For<DeleteConfirmationModalComponent>(Substitute.For<IDeleteCharacterDataRequestService>(), Substitute.For<ICharacterRegistry>(), Substitute.For<ILocalizationService>()),
            this.mockClientState
        );
    }

    [Fact]
    public void DrawConditions_ReturnsTrue_WhenClientIsLoggedIn() {
        // Arrange
        this.mockClientState.IsLoggedIn.Returns(true);

        // Act
        bool canDraw = this.mainWindow.DrawConditions();

        // Assert
        Assert.True(canDraw);
    }

    [Fact]
    public void DrawConditions_ReturnsFalse_WhenClientIsNotLoggedIn() {
        // Arrange
        this.mockClientState.IsLoggedIn.Returns(false);

        // Act
        bool canDraw = this.mainWindow.DrawConditions();

        // Assert
        Assert.False(canDraw);
    }

    [Fact]
    public void OnLogout_ClosesWindow() {
        // Arrange
        this.mainWindow.IsOpen = true;

        // Act
        // Use the specific delegate type defined by Dalamud's IClientState
        this.mockClientState.Logout += Raise.Event<IClientState.LogoutDelegate>(0, 0);

        // Assert
        Assert.False(this.mainWindow.IsOpen);
    }

    [Fact]
    public void OnOpen_ClosesWindowAndPreventsRefresh_WhenNotLoggedIn() {
        // Arrange
        this.mainWindow.IsOpen = true;
        this.mockClientState.IsLoggedIn.Returns(false);

        // Act
        this.mainWindow.OnOpen();

        // Assert
        Assert.False(this.mainWindow.IsOpen);
    }
}