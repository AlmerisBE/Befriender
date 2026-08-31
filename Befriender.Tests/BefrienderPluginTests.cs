namespace Befriender.Tests;

using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.IO;
using Xunit;

public class BefrienderPluginTests {
    [Fact]
    public void Plugin_OnInitialization_BuildsDependencyInjectionWithoutErrors() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(Path.GetTempPath()));

        var mockChatGui = Substitute.For<IChatGui>();
        var mockCommandManager = Substitute.For<ICommandManager>();

        var mockClientState = Substitute.For<IClientState>();
        mockClientState.IsLoggedIn.Returns(false); // Simulate title screen

        var mockLogger = Substitute.For<IPluginLog>();
        var mockFramework = Substitute.For<IFramework>();

        var mockObjectTable = Substitute.For<IObjectTable>();
        mockObjectTable.LocalPlayer.Returns((IPlayerCharacter)null!); // Simulate unallocated memory

        var mockDataManager = Substitute.For<IDataManager>();
        var mockTextureProvider = Substitute.For<ITextureProvider>();
        var mockGameInteropProvider = Substitute.For<IGameInteropProvider>();
        var mockAddonLifecycle = Substitute.For<IAddonLifecycle>();
        var mockKeyState = Substitute.For<IKeyState>();
        var mockNotificationManager = Substitute.For<INotificationManager>();

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