namespace Befriender.Tests.Core.GameData.Services;

using Befriender.Core.GameData.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class GameDataServiceTests {
    [Fact]
    public void GameDataService_GetGrandCompanyIconId_ReturnsCorrectIcons() {
        // Arrange
        var mockDataManager = Substitute.For<IDataManager>();
        var service = new GameDataService(mockDataManager);

        // Act & Assert
        Assert.Equal(60501u, service.GetGrandCompanyIconId(1)); // Maelstrom
        Assert.Equal(60502u, service.GetGrandCompanyIconId(2)); // Twin Adder
        Assert.Equal(60503u, service.GetGrandCompanyIconId(3)); // Immortal Flames
        Assert.Equal(0u, service.GetGrandCompanyIconId(0));     // None
        Assert.Equal(0u, service.GetGrandCompanyIconId(99));    // Invalid
    }
}