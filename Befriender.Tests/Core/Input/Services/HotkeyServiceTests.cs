namespace Befriender.Tests.Core.Input.Services;

using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.UI.Input.Services;
using Dalamud.Game.ClientState.Keys;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class HotkeyServiceTests {
    [Fact]
    public void OnUpdate_TriggersEvent_WhenKeyAndModifiersMatch() {
        // Arrange
        var mockKeyState = Substitute.For<IKeyState>();
        var mockFramework = Substitute.For<IFramework>();
        var mockConfigService = Substitute.For<IConfigurationService>();

        var config = new PluginConfiguration {
            Hotkey = VirtualKey.F,
            HotkeyCtrl = true,
            HotkeyShift = false,
            HotkeyAlt = false
        };
        mockConfigService.GetConfig().Returns(config);

        mockKeyState[VirtualKey.F].Returns(true);
        mockKeyState[VirtualKey.CONTROL].Returns(true);
        mockKeyState[VirtualKey.SHIFT].Returns(false);
        mockKeyState[VirtualKey.MENU].Returns(false);

        using var service = new HotkeyService(mockKeyState, mockFramework, mockConfigService);
        bool eventFired = false;
        service.OnHotkeyPressed += () => eventFired = true;

        // Act
        // On simule l'appel de l'event Update du framework
        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        // Assert
        Assert.True(eventFired);
    }
}