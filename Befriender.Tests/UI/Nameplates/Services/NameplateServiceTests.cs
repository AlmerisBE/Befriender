namespace Befriender.Tests.UI.Nameplates.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Configuration.Contracts;
using Befriender.UI.Nameplates.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class NameplateServiceTests {
    [Fact]
    public void Constructor_SubscribesToNamePlateUpdate_AndDisposesCleanly() {
        var mockNamePlateGui = Substitute.For<INamePlateGui>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockConfig = Substitute.For<IConfigurationService>();

        var service = new NameplateService(mockNamePlateGui, mockRegistry, mockConfig);

        mockNamePlateGui.Received(1).OnNamePlateUpdate += Arg.Any<INamePlateGui.OnPlateUpdateDelegate>();

        service.Dispose();

        mockNamePlateGui.Received(1).OnNamePlateUpdate -= Arg.Any<INamePlateGui.OnPlateUpdateDelegate>();
    }
}