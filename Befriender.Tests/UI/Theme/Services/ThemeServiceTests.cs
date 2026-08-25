namespace Befriender.Tests.UI.Theme.Services;

using Befriender.UI.Theme.Services;
using Xunit;

public class ThemeServiceTests {
    [Fact]
    public void ThemeService_Initialization_ProvidesDefaultDarkThemePalette() {
        // Arrange & Act
        var service = new ThemeService();
        var palette = service.CurrentPalette;

        // Assert
        Assert.NotNull(palette);
        Assert.Equal(1.0f, palette.TextOnline.X); // White text check
        Assert.Equal(1.0f, palette.TextOnline.W); // Full opacity check
        Assert.Equal(0.8f, palette.TextDeleted.X); // Reddish text check
    }
}