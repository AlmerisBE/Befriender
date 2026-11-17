namespace Befriender.Tests.Core.Sources.Friends;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.Sources.Friends;
using Befriender.Core.Sources.Friends.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class FriendListSourceTests {
    private IFriendListScanner mockScanner;
    private IFramework mockFramework;
    private IClientState mockClientState;
    private IConfigurationService mockConfigService;

    public FriendListSourceTests() {
        this.mockScanner = Substitute.For<IFriendListScanner>();
        this.mockFramework = Substitute.For<IFramework>();
        this.mockClientState = Substitute.For<IClientState>();
        this.mockConfigService = Substitute.For<IConfigurationService>();
    }

    private FriendListSource CreateSource() {
        return new FriendListSource(
            this.mockScanner,
            this.mockFramework,
            this.mockClientState,
            this.mockConfigService);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void RequestRefresh_ExecutesSilentServerUpdate_RegardlessOfUserInitiation(bool isUserInitiated) {
        // Arrange
        this.mockConfigService.GetConfig().Returns(new PluginConfiguration { SyncOnLogin = true });
        var source = this.CreateSource();

        // Act
        source.RequestRefresh(isUserInitiated);

        // Assert
        this.mockScanner.Received(1).RequestServerUpdate();
        Assert.True(source.IsSyncing);
    }
}