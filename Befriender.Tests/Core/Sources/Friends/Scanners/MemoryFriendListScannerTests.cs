namespace Befriender.Tests.Core.Sources.Friends.Scanners;

using FFXIVClientStructs.FFXIV.Client.UI.Info;
using global::Befriender.Core.Sources.Friends.Scanners;
using Xunit;

public unsafe class MemoryFriendListScannerTests {
    [Fact]
    public void ParseEntry_CorrectlyMapsNativeStructToDomainModel() {
        var scanner = new MemoryFriendListScanner();

        var entryData = new InfoProxyCommonList.CharacterData {
            ContentId = 123456789,
            HomeWorld = 33,
            CurrentWorld = 44,
            State = (InfoProxyCommonList.CharacterData.OnlineStatus)47,
            Job = 1,
            Location = 129,
            Languages = (InfoProxyCommonList.CharacterData.LanguageMask)2,
            GrandCompany = (FFXIVClientStructs.FFXIV.Client.UI.Agent.GrandCompany)1
        };

        var parsed = scanner.ParseEntry(&entryData);

        Assert.NotNull(parsed);
        Assert.Equal(123456789ul, parsed.ContentId);
        Assert.Equal(33u, parsed.HomeWorldId);
        Assert.Equal(44u, parsed.CurrentWorldId);
        Assert.True(parsed.IsOnline);
        Assert.Equal(47ul, parsed.OnlineStateMask);
        Assert.Equal(1, parsed.JobId);
        Assert.Equal(129u, parsed.LocationId);
        Assert.Equal(2, parsed.ClientLanguages);
        Assert.Equal(1, parsed.GrandCompany);
    }

    [Fact]
    public void ParseEntry_ReturnsNull_WhenPointerIsNull() {
        var scanner = new MemoryFriendListScanner();

        var parsed = scanner.ParseEntry(null);

        Assert.Null(parsed);
    }
}