namespace Befriender.Tests.Core.GameData.Services;

using Befriender.Core.GameData.Services;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using NSubstitute;
using Xunit;

public class GameDataServiceTests {
    [Fact]
    public void GameDataService_GetGrandCompanyIconId_ReturnsCorrectIcons() {
        // Arrange
        var mockDataManager = Substitute.For<IDataManager>();
        var service = new GameDataService(mockDataManager);

        // Act & Assert
        Assert.Equal(60871u, service.GetGrandCompanyIconId((byte)FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.Maelstrom));
        Assert.Equal(60872u, service.GetGrandCompanyIconId((byte)FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.TwinAdder));
        Assert.Equal(60873u, service.GetGrandCompanyIconId((byte)FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.ImmortalFlames));
        Assert.Equal(0u, service.GetGrandCompanyIconId(0));
        Assert.Equal(0u, service.GetGrandCompanyIconId(99));
    }

    [Fact]
    public void GameDataService_GetGrandCompanyName_ReturnsFallbackIfSheetFails() {
        // Arrange
        var mockDataManager = Substitute.For<IDataManager>();
        mockDataManager.GetExcelSheet<Lumina.Excel.Sheets.GrandCompany>().Returns((ExcelSheet<Lumina.Excel.Sheets.GrandCompany>)null!);

        var service = new GameDataService(mockDataManager);

        // Act
        var result1 = service.GetGrandCompanyName(1);
        var result0 = service.GetGrandCompanyName(0);

        // Assert
        Assert.Equal("1", result1);
        Assert.Equal(string.Empty, result0);
    }
}