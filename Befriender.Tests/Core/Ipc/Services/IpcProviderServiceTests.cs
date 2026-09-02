namespace Befriender.Tests.Core.Ipc.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Ipc.Services;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class IpcProviderServiceTests {
    [Fact]
    public void Initialize_RegistersAllIpcProviders() {
        var mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockLog = Substitute.For<IPluginLog>();

        mockPluginInterface.GetIpcProvider<string>("Befriender.GetCharacters")
            .Returns(Substitute.For<ICallGateProvider<string>>());
        mockPluginInterface.GetIpcProvider<string, string, int, bool>("Befriender.RegisterSource")
            .Returns(Substitute.For<ICallGateProvider<string, string, int, bool>>());
        mockPluginInterface.GetIpcProvider<string, string, bool>("Befriender.UpdateSourceData")
            .Returns(Substitute.For<ICallGateProvider<string, string, bool>>());

        using var service = new IpcProviderService(mockPluginInterface, mockRegistry, mockLog);

        service.Initialize();

        mockPluginInterface.Received(1).GetIpcProvider<string>("Befriender.GetCharacters");
        mockPluginInterface.Received(1).GetIpcProvider<string, string, int, bool>("Befriender.RegisterSource");
        mockPluginInterface.Received(1).GetIpcProvider<string, string, bool>("Befriender.UpdateSourceData");
    }
}