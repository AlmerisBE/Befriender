namespace Befriender.Tests.UI.FriendList.Windows;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.Input.Contracts;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.MainWindow.Contracts;
using Befriender.UI.MainWindow.Windows;
using Befriender.UI.Theme.Contracts;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class MainWindowTests {
    [Fact]
    public void MainWindow_Initialization_DoesNotThrow() {
        var mockTabs = new List<ITab>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockConfig = Substitute.For<IConfigurationService>();
        var mockLoc = Substitute.For<ILocalizationService>();
        var mockNav = Substitute.For<IWindowNavigationService>();

        var mockSource = Substitute.For<ICharacterSource>();
        var mockStatusBar = new FriendStatusBarComponent(mockRegistry, new[] { mockSource }, mockLoc);

        var mockRemoveRequest = Substitute.For<IRemoveCharacterRequestService>();
        var mockRemoveModal = new RemoveConfirmationModalComponent(mockRemoveRequest, mockRegistry, mockLoc);

        var mockHotkey = Substitute.For<IHotkeyService>();
        var mockTheme = Substitute.For<IThemeService>();

        // 1. Déclarer le mock pour le nouveau service de suppression
        var mockDeleteRequest = Substitute.For<IDeleteCharacterDataRequestService>();

        // 2. Instancier le composant modal avec le service mocké
        var deleteModal = new DeleteConfirmationModalComponent(mockDeleteRequest, mockRegistry, mockLoc);

        // 3. Passer le composant modal en dernier paramètre
        var exception = Record.Exception(() => new MainWindow(
            mockTabs,
            mockRegistry,
            mockConfig,
            mockLoc,
            mockNav,
            mockStatusBar,
            mockRemoveModal,
            mockHotkey,
            mockTheme,
            deleteModal
        ));

        Assert.Null(exception);
    }
}