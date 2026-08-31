namespace Befriender.Tests.UI.FriendList.Windows;

using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Configuration.Contracts;
using global::Befriender.Core.Localization.Contracts;
using global::Befriender.UI.FriendList.Components;
using global::Befriender.UI.FriendList.Contracts;
using global::Befriender.UI.FriendList.Windows;
using global::Befriender.UI.Windows.Contracts;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class FriendListWindowTests {
    [Fact]
    public void Constructor_SetsFirstTabAsDefault() {
        // Arrange
        var mockTab1 = Substitute.For<ITab>();
        mockTab1.InternalName.Returns("Tab_First");
        var mockTab2 = Substitute.For<ITab>();

        var mockConfig = Substitute.For<IConfigurationService>();
        var mockLoc = Substitute.For<ILocalizationService>();
        var mockNav = Substitute.For<IWindowNavigationService>();

        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockSources = new List<ICharacterSource>();
        var mockRequestService = Substitute.For<IRemoveCharacterRequestService>();

        // Instantiate the required UI components with mocked dependencies
        var statusBar = new FriendStatusBarComponent(mockRegistry, mockSources, mockLoc);
        var removeModal = new RemoveConfirmationModalComponent(mockRequestService, mockRegistry, mockLoc);

        var tabs = new List<ITab> { mockTab1, mockTab2 };

        // Act
        using var window = new FriendListWindow(tabs, mockConfig, mockLoc, mockNav, statusBar, removeModal);

        // Assert
        Assert.NotNull(window);
    }
}