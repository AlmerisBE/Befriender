namespace Befriender.Tests.Core.Sources.FreeCompany;

using Dalamud.Plugin.Services;
using global::Befriender.Core.Sources.FreeCompany;
using global::Befriender.Core.Sources.FreeCompany.Contracts;
using NSubstitute;
using System.Threading;
using Xunit;

public class FreeCompanySourceTests {
    [Fact]
    public void OnFrameworkUpdate_DebouncesHashChanges_AndFiresDataUpdatedOnlyWhenStabilized() {
        // Arrange
        var mockScanner = Substitute.For<IFreeCompanyScanner>();
        var mockFramework = Substitute.For<IFramework>();

        using var source = new FreeCompanySource(mockScanner, mockFramework);

        bool eventFired = false;
        source.DataUpdated += () => eventFired = true;

        // Act - Simulate a native hash change
        mockScanner.GetStateHash().Returns(54321ul);
        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        // Assert - Event shouldn't fire immediately due to debounce logic
        Assert.False(eventFired);

        // Simulate waiting past the 1-second stabilization time
        Thread.Sleep(1100);

        // Act - Next update tick should now trigger the refresh
        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        // Assert - Event is fired now that data is stable
        Assert.True(eventFired);
    }

    [Fact]
    public void IsSyncing_ReturnsTrue_WhenManualRefreshIsTriggered() {
        // Arrange
        var mockScanner = Substitute.For<IFreeCompanyScanner>();
        var mockFramework = Substitute.For<IFramework>();
        using var source = new FreeCompanySource(mockScanner, mockFramework);

        Assert.False(source.IsSyncing);

        // Act
        source.TriggerManualRefresh();

        // Assert
        Assert.True(source.IsSyncing);
        mockScanner.Received(1).RequestServerUpdate();
    }
}