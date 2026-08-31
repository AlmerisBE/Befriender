namespace Befriender.Tests.Core.GameData.Services;

using Befriender.Core.GameData.Services;
using Befriender.UI.Localization.Contracts;
using Dalamud.Plugin.Services;
using Lumina.Excel;
using NSubstitute;
using Xunit;

public class GameDataServiceTests {
    [Fact]
    public void GameDataService_GetGrandCompanyIconId_ReturnsCorrectIcons() {
        var mockDataManager = Substitute.For<IDataManager>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockLoc = Substitute.For<ILocalizationService>();
        var service = new GameDataService(mockDataManager, mockObjectTable, mockLoc);

        Assert.Equal(60871u, service.GetGrandCompanyIconId((byte)FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.Maelstrom));
        Assert.Equal(60872u, service.GetGrandCompanyIconId((byte)FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.TwinAdder));
        Assert.Equal(60873u, service.GetGrandCompanyIconId((byte)FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany.ImmortalFlames));
        Assert.Equal(0u, service.GetGrandCompanyIconId(0));
        Assert.Equal(0u, service.GetGrandCompanyIconId(99));
    }

    [Fact]
    public void GameDataService_GetGrandCompanyName_ReturnsFallbackIfSheetFails() {
        var mockDataManager = Substitute.For<IDataManager>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockLoc = Substitute.For<ILocalizationService>();
        mockDataManager.GetExcelSheet<Lumina.Excel.Sheets.GrandCompany>().Returns((ExcelSheet<Lumina.Excel.Sheets.GrandCompany>)null!);

        var service = new GameDataService(mockDataManager, mockObjectTable, mockLoc);

        Assert.Equal("1", service.GetGrandCompanyName(1));
        Assert.Equal(string.Empty, service.GetGrandCompanyName(0));
    }

    [Fact]
    public void GameDataService_IsCrossWorld_ReturnsFalseIfPlayerIsInDuty() {
        var mockDataManager = Substitute.For<IDataManager>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockLoc = Substitute.For<ILocalizationService>();
        var service = new GameDataService(mockDataManager, mockObjectTable, mockLoc);

        ulong inDutyMask = (ulong)FFXIVClientStructs.FFXIV.Client.UI.Info.InfoProxyCommonList.CharacterData.OnlineStatus.InDuty;

        // We supply 0 as locationId to satisfy the new signature
        var result = service.IsCrossWorld(33, 0, inDutyMask, 0);

        Assert.False(result);
    }
}