namespace Befriender.Tests;

using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.IO;
using Xunit;

public class BefrienderPluginTests {
    [Fact]
    public void Plugin_OnInitialization_BuildsDependencyInjectionWithoutErrors() {
        // Arrange
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();

        // We provide a dummy directory to avoid NullReferenceException in storage services
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(Path.GetTempPath()));

        var mockChatGui = Substitute.For<IChatGui>();
        var mockCommandManager = Substitute.For<ICommandManager>();
        var mockClientState = Substitute.For<IClientState>();
        var mockLogger = Substitute.For<IPluginLog>();
        var mockFramework = Substitute.For<IFramework>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockDataManager = Substitute.For<IDataManager>();
        var mockTextureProvider = Substitute.For<ITextureProvider>();
        var mockGameInteropProvider = Substitute.For<IGameInteropProvider>();
        var mockAddonLifecycle = Substitute.For<IAddonLifecycle>();
        var mockKeyState = Substitute.For<IKeyState>();
        var mockNotificationManager = Substitute.For<INotificationManager>();

        // Act & Assert
        var exception = Record.Exception(() => new BefrienderPlugin(
            mockPluginInterface,
            mockChatGui,
            mockCommandManager,
            mockClientState,
            mockLogger,
            mockFramework,
            mockObjectTable,
            mockDataManager,
            mockTextureProvider,
            mockGameInteropProvider,
            mockAddonLifecycle,
            mockKeyState,
            mockNotificationManager));

        Assert.Null(exception);
    }
}