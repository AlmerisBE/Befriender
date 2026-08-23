namespace Befriender.Tests;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class BefrienderPluginTests {
    [Fact]
    public void Plugin_OnInitialization_BuildsDependencyInjectionWithoutErrors() {
        // Arrange
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockChatGui = Substitute.For<IChatGui>();
        var mockCommandManager = Substitute.For<ICommandManager>();
        var mockClientState = Substitute.For<IClientState>();
        var mockLogger = Substitute.For<IPluginLog>();
        var mockFramework = Substitute.For<IFramework>(); // Création du mock IFramework

        // Act & Assert
        // On passe mockFramework en dernier paramètre
        var exception = Record.Exception(() => new BefrienderPlugin(mockPluginInterface, mockChatGui, mockCommandManager, mockClientState, mockLogger, mockFramework));

        Assert.Null(exception);
    }
}