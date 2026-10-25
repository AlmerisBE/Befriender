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
        mockClientState.IsLoggedIn.Returns(false);

        var mockLogger = Substitute.For<IPluginLog>();
        var mockFramework = Substitute.For<IFramework>();

        var mockObjectTable = Substitute.For<IObjectTable>();
        mockObjectTable.LocalPlayer.Returns((IPlayerCharacter)null!);

        var mockDataManager = Substitute.For<IDataManager>();
        var mockTextureProvider = Substitute.For<ITextureProvider>();
        var mockGameInteropProvider = Substitute.For<IGameInteropProvider>();
        var mockAddonLifecycle = Substitute.For<IAddonLifecycle>();
        var mockKeyState = Substitute.For<IKeyState>();
        var mockNotificationManager = Substitute.For<INotificationManager>();
        var mockGameGui = Substitute.For<IGameGui>();
        var mockContextMenu = Substitute.For<IContextMenu>();
        var mockNamePlateGui = Substitute.For<INamePlateGui>();
        var mockCondition = Substitute.For<ICondition>();

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
            mockNotificationManager,
            mockGameGui,
            mockContextMenu,
            mockNamePlateGui,
            mockCondition));

        Assert.Null(exception);
    }
}