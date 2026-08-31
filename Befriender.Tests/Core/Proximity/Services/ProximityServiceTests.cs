namespace Befriender.Tests.Core.Proximity.Services;

using Dalamud.Plugin.Services;
using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Configuration.Contracts;
using global::Befriender.Core.Localization.Contracts;
using global::Befriender.Core.Proximity.Services;
using NSubstitute;
using Xunit;

public class ProximityServiceTests {
    [Fact]
    public void Constructor_SubscribesToFrameworkUpdate_AndDisposesCleanly() {
        // Arrange
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockClientState = Substitute.For<IClientState>();
        var mockConfig = Substitute.For<IConfigurationService>();
        var mockNotif = Substitute.For<INotificationManager>();
        var mockLoc = Substitute.For<ILocalizationService>();
        var mockFramework = Substitute.For<IFramework>();

        // Act
        var service = new ProximityService(
            mockRegistry,
            mockObjectTable,
            mockClientState,
            mockConfig,
            mockNotif,
            mockLoc,
            mockFramework);

        // Assert Subscription
        mockFramework.Received(1).Update += Arg.Any<IFramework.OnUpdateDelegate>();

        // Act Dispose
        service.Dispose();

        // Assert Unsubscription
        mockFramework.Received(1).Update -= Arg.Any<IFramework.OnUpdateDelegate>();
    }
}