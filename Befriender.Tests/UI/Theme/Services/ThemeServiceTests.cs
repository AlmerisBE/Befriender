namespace Befriender.Tests.UI.Theme.Services;

using Dalamud.Plugin;
using global::Befriender.Core.Configuration.Contracts;
using global::Befriender.Core.Configuration.Models;
using global::Befriender.UI.Theme.Services;
using NSubstitute;
using System;
using System.IO;
using Xunit;

public class ThemeServiceTests : IDisposable {
    private IConfigurationService mockConfigService;
    private IDalamudPluginInterface mockPluginInterface;
    private string tempDirectory;

    public ThemeServiceTests() {
        this.mockConfigService = Substitute.For<IConfigurationService>();
        this.mockPluginInterface = Substitute.For<IDalamudPluginInterface>();

        this.tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.tempDirectory);

        this.mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(this.tempDirectory));
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { SelectedThemeName = "Dark" });
    }

    [Fact]
    public void Constructor_CreatesDefaultThemes_AndLoadsThem() {
        var service = new ThemeService(this.mockConfigService, this.mockPluginInterface);

        var availableThemes = service.GetAvailableThemes();

        Assert.Contains("Dark", availableThemes);
        Assert.Contains("Light", availableThemes);
        Assert.Equal("Dark", service.CurrentThemeName);
    }

    [Fact]
    public void SetTheme_UpdatesConfig_AndChangesCurrentPalette() {
        var service = new ThemeService(this.mockConfigService, this.mockPluginInterface);

        service.SetTheme("Light");

        Assert.Equal("Light", service.CurrentThemeName);
        this.mockConfigService.Received(1).Save();
    }

    [Fact]
    public void SetTheme_FallsBackToDark_WhenThemeIsInvalid() {
        var service = new ThemeService(this.mockConfigService, this.mockPluginInterface);

        service.SetTheme("NonExistentTheme");

        Assert.Equal("Dark", service.CurrentThemeName);
    }

    public void Dispose() {
        if (Directory.Exists(this.tempDirectory)) Directory.Delete(this.tempDirectory, true);
    }
}