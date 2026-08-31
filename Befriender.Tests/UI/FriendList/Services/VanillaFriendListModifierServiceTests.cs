namespace Befriender.Tests.UI.FriendList.Services;

using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Plugin.Services;
using global::Befriender.Core.Characters.Contracts;
using global::Befriender.UI.FriendList.Services;
using NSubstitute;
using Xunit;

public class VanillaFriendListModifierServiceTests {
    [Fact]
    public void Constructor_RegistersLifecycleListener() {
        var mockLifecycle = Substitute.For<IAddonLifecycle>();
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockLog = Substitute.For<IPluginLog>();

        using var service = new VanillaFriendListModifierService(mockLifecycle, mockRegistry, mockLog);

        mockLifecycle.Received(1).RegisterListener(AddonEvent.PreDraw, "FriendList", Arg.Any<AddonLifecycle.OnEvent>());
    }
}