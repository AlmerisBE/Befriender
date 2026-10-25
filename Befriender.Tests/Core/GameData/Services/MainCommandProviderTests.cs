namespace Befriender.Tests.Core.GameData.Services;

using Befriender.Core.GameData.Services;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using NSubstitute;
using Xunit;

public class MainCommandProviderTests {
    [Fact]
    public void GetAllMainCommands_ReturnsEmpty_WhenSheetIsNotFound() {
        // Arrange
        var mockDataManager = Substitute.For<IDataManager>();
        mockDataManager.GetExcelSheet<MainCommand>().Returns((Lumina.Excel.ExcelSheet<MainCommand>?)null);

        var provider = new MainCommandProvider(mockDataManager);

        // Act
        var result = provider.GetAllMainCommands();

        // Assert
        Assert.Empty(result);
    }
}