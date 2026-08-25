namespace Befriender.Tests.UI.Theme.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.UI.Theme.Services;
using Dalamud.Plugin;
using NSubstitute;
using System.IO;
using Xunit;

public class ThemeServiceTests {

    [Fact]
    public void ThemeService_Initialization_GeneratesAndLoadsThemesFromDisk() {
        // Arrange
        var mockConfigService = Substitute.For<IConfigurationService>();
        mockConfigService.GetConfig().Returns(new PluginConfiguration { SelectedThemeName = "Light" });

        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var fakePath = Path.GetTempPath();
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(fakePath));

        // Act
        var service = new ThemeService(mockConfigService, mockPluginInterface);

        // Assert
        Assert.Equal("Light", service.CurrentThemeName);
        Assert.Contains("Dark", service.GetAvailableThemes());
        Assert.Contains("Light", service.GetAvailableThemes());
        Assert.Equal(Path.Combine(fakePath, "Themes"), service.ThemesDirectory);
    }

    [Fact]
    public void ThemeService_SetTheme_ChangesPaletteAndSavesConfig() {
        // Arrange
        var mockConfigService = Substitute.For<IConfigurationService>();
        var config = new PluginConfiguration { SelectedThemeName = "Dark" };
        mockConfigService.GetConfig().Returns(config);

        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(Path.GetTempPath()));

        var service = new ThemeService(mockConfigService, mockPluginInterface);

        // Act
        service.SetTheme("Light");

        // Assert
        Assert.Equal("Light", service.CurrentThemeName);
        Assert.Equal("Light", config.SelectedThemeName);
        mockConfigService.Received(1).Save();
    }
}