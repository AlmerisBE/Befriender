namespace Befriender.Tests.Core.Sources.Friends;

using Befriender.Core.Sources.Friends;
using Befriender.Core.Sources.Friends.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.Threading;
using Xunit;

public class FriendListSourceTests {
    [Fact]
    public void OnFrameworkUpdate_DebouncesHashChanges_AndFiresDataUpdatedOnlyWhenStabilized() {
        var mockScanner = Substitute.For<IFriendListScanner>();
        var mockFramework = Substitute.For<IFramework>();
        var mockClientState = Substitute.For<IClientState>();

        using var source = new FriendListSource(mockScanner, mockFramework, mockClientState);

        bool eventFired = false;
        source.DataUpdated += () => eventFired = true;

        mockScanner.GetStateHash().Returns(12345ul);
        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        Assert.False(eventFired);

        Thread.Sleep(1100);

        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        Assert.True(eventFired);
    }

    [Fact]
    public void IsSyncing_ReturnsTrue_WhenManualRefreshIsTriggered() {
        var mockScanner = Substitute.For<IFriendListScanner>();
        var mockFramework = Substitute.For<IFramework>();
        var mockClientState = Substitute.For<IClientState>();

        using var source = new FriendListSource(mockScanner, mockFramework, mockClientState);

        Assert.False(source.IsSyncing);

        source.TriggerManualRefresh();

        Assert.True(source.IsSyncing);
        mockScanner.Received(1).RequestServerUpdate();
    }
}