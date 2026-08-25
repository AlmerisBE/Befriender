namespace Befriender.Tests.UI.Theme.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.UI.Theme.Models;
using Befriender.UI.Theme.Services;
using NSubstitute;
using Xunit;

public class ThemeServiceTests {
    [Fact]
    public void ThemeService_Initialization_LoadsThemeFromConfiguration() {
        // Arrange
        var mockConfigService = Substitute.For<IConfigurationService>();
        mockConfigService.GetConfig().Returns(new PluginConfiguration { SelectedTheme = (int)ThemeStyle.Light });

        // Act
        var service = new ThemeService(mockConfigService);

        // Assert
        Assert.Equal(ThemeStyle.Light, service.CurrentStyle);
        // Validates specific Light theme parameter applied (Text color)
        Assert.Equal(0.15f, service.CurrentPalette.Text.X);
    }

    [Fact]
    public void ThemeService_SetTheme_ChangesPaletteAndSavesConfig() {
        // Arrange
        var mockConfigService = Substitute.For<IConfigurationService>();
        var config = new PluginConfiguration { SelectedTheme = (int)ThemeStyle.Dark };
        mockConfigService.GetConfig().Returns(config);

        var service = new ThemeService(mockConfigService);

        // Act
        service.SetTheme(ThemeStyle.Light);

        // Assert
        Assert.Equal(ThemeStyle.Light, service.CurrentStyle);
        Assert.Equal(1, config.SelectedTheme);
        mockConfigService.Received(1).Save();
    }
}