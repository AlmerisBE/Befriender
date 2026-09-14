namespace Befriender.Tests.Core.Ipc.Services;

using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Ipc.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class DynamicIpcSourceTests {
    [Fact]
    public void UpdateState_ReplacesCurrentState_AndFiresEvent() {
        var sourceId = Guid.NewGuid();
        var source = new DynamicIpcSource(sourceId, "TestIPC", 15);

        bool eventFired = false;
        source.DataUpdated += () => eventFired = true;

        var chars = new List<Character> { new Character { Name = "IpcUser" } };

        source.UpdateState(chars);

        Assert.True(eventFired);
        Assert.Single(source.GetCurrentState());
        Assert.Equal("IpcUser", source.GetCurrentState().First().Name);
        Assert.Equal(15, source.Priority);
    }

    [Fact]
    public void RequestManualRefresh_DoesNotThrowException() {
        var source = new DynamicIpcSource(Guid.NewGuid(), "TestIPC", 15);

        var exception = Record.Exception(() => source.RequestManualRefresh());

        Assert.Null(exception);
    }
}