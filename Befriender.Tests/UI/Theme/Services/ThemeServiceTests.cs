namespace Befriender.Tests.UI.Theme.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.UI.Theme.Services;
using Dalamud.Plugin;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using Xunit;

public class ThemeServiceTests : IDisposable {
    private List<string> createdDirectories = new();

    private string GetUniqueTempPath() {
        var path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(path);
        this.createdDirectories.Add(path);
        return path;
    }

    [Fact]
    public void ThemeService_Initialization_GeneratesAndLoadsThemesFromDisk() {
        // Arrange
        var mockConfigService = Substitute.For<IConfigurationService>();
        mockConfigService.GetConfig().Returns(new PluginConfiguration { SelectedThemeName = "Light" });

        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var fakePath = this.GetUniqueTempPath(); // Utilisation d'un chemin unique
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
        var fakePath = this.GetUniqueTempPath(); // Utilisation d'un chemin unique
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(fakePath));

        var service = new ThemeService(mockConfigService, mockPluginInterface);

        // Act
        service.SetTheme("Light");

        // Assert
        Assert.Equal("Light", service.CurrentThemeName);
        Assert.Equal("Light", config.SelectedThemeName);
        mockConfigService.Received(1).Save();
    }

    public void Dispose() {
        // Nettoyage des dossiers temporaires après l'exécution des tests
        foreach (var dir in this.createdDirectories) {
            if (Directory.Exists(dir)) {
                try {
                    Directory.Delete(dir, true);
                }
                catch {
                    // Ignore de potentielles erreurs de nettoyage dans l'environnement de test
                }
            }
        }
    }
}