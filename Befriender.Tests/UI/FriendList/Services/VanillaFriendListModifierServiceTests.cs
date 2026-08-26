namespace Befriender.Tests.UI.FriendList.Services;

using Befriender.Core.Friends.Contracts;
using Befriender.UI.FriendList.Services;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Plugin.Services;
using NSubstitute;
using Xunit;

public class VanillaFriendListModifierServiceTests {
    [Fact]
    public void Service_OnInitialization_RegistersLifecycleListenerOnPreDraw() {
        // Arrange
        var mockLifecycle = Substitute.For<IAddonLifecycle>();
        var mockRepo = Substitute.For<IFriendRepository>();
        var mockLog = Substitute.For<IPluginLog>();

        // Act
        using var service = new VanillaFriendListModifierService(mockLifecycle, mockRepo, mockLog);

        // Assert
        // The addon name must perfectly match the production code implementation ("FriendList")
        mockLifecycle.Received(1).RegisterListener(AddonEvent.PreDraw, "FriendList", Arg.Any<IAddonLifecycle.AddonEventDelegate>());
    }

    [Fact]
    public void Service_OnDispose_UnregistersLifecycleListenerOnPreDraw() {
        // Arrange
        var mockLifecycle = Substitute.For<IAddonLifecycle>();
        var mockRepo = Substitute.For<IFriendRepository>();
        var mockLog = Substitute.For<IPluginLog>();
        var service = new VanillaFriendListModifierService(mockLifecycle, mockRepo, mockLog);

        // Act
        service.Dispose();

        // Assert
        mockLifecycle.Received(1).UnregisterListener(AddonEvent.PreDraw, "FriendList", Arg.Any<IAddonLifecycle.AddonEventDelegate>());
    }
}