namespace Befriender.Tests.Core.Configuration.Services;

using Befriender.Core.Configuration.Models;
using Befriender.Core.Configuration.Services;
using Dalamud.Configuration;
using Dalamud.Plugin;
using NSubstitute;
using Xunit;

public class ConfigurationServiceTests {

    [Fact]
    public void ConfigurationService_Initialization_LoadsExistingConfig() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var existingConfig = new PluginConfiguration {
            Version = 1,
            MinSyncIntervalMinutes = 20,
            MaxSyncIntervalMinutes = 40
        };

        mockPluginInterface.GetPluginConfig().Returns(existingConfig);

        var service = new ConfigurationService(mockPluginInterface);
        var config = service.GetConfig();

        Assert.NotNull(config);
        Assert.Equal(20, config.MinSyncIntervalMinutes);
        Assert.Equal(40, config.MaxSyncIntervalMinutes);
        Assert.Equal(1, config.Version);
    }

    [Fact]
    public void ConfigurationService_Initialization_CreatesNewConfigIfNull() {
        // Arrange
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();

        mockPluginInterface.GetPluginConfig().Returns((IPluginConfiguration)null!);

        // Act
        var service = new ConfigurationService(mockPluginInterface);
        var config = service.GetConfig();

        // Assert
        Assert.NotNull(config);
        Assert.Equal(15, config.MinSyncIntervalMinutes);
        Assert.Equal(30, config.MaxSyncIntervalMinutes);
        Assert.Equal(0, config.Version);
    }

    [Fact]
    public void ConfigurationService_Save_PassesConfigToDalamud() {
        // Arrange
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var service = new ConfigurationService(mockPluginInterface);
        var config = service.GetConfig();

        config.MinSyncIntervalMinutes = 15;
        config.MaxSyncIntervalMinutes = 30;

        // Act
        service.Save();

        // Assert
        mockPluginInterface.Received(1).SavePluginConfig(config);
    }
}