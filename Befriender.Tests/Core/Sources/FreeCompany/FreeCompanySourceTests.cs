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
        var mockScanner = Substitute.For<IFreeCompanyScanner>();
        var mockFramework = Substitute.For<IFramework>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        using var source = new FreeCompanySource(mockScanner, mockFramework, mockObjectTable);

        bool eventFired = false;
        source.DataUpdated += () => eventFired = true;

        mockScanner.GetStateHash().Returns(54321ul);
        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        Assert.False(eventFired);

        Thread.Sleep(1100);

        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        Assert.True(eventFired);
    }

    [Fact]
    public void IsSyncing_ReturnsTrue_WhenManualRefreshIsTriggered() {
        var mockScanner = Substitute.For<IFreeCompanyScanner>();
        var mockFramework = Substitute.For<IFramework>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        using var source = new FreeCompanySource(mockScanner, mockFramework, mockObjectTable);

        Assert.False(source.IsSyncing);

        source.TriggerManualRefresh();

        Assert.True(source.IsSyncing);
        mockScanner.Received(1).RequestServerUpdate();
    }
}