namespace Befriender.Tests.Core.Sources.FreeCompany.Scanners;

using FFXIVClientStructs.FFXIV.Client.UI.Info;
using global::Befriender.Core.Sources.FreeCompany.Scanners;
using Xunit;

public unsafe class MemoryFreeCompanyScannerTests {
    [Fact]
    public void ParseEntry_CorrectlyMapsNativeStructToDomainModel() {
        var scanner = new MemoryFreeCompanyScanner();

        var entryData = new InfoProxyCommonList.CharacterData {
            ContentId = 987654321,
            HomeWorld = 33,
            CurrentWorld = 33,
            State = 0,
            Job = 3,
            Location = 130
        };

        var parsed = scanner.ParseEntry(&entryData);

        Assert.NotNull(parsed);
        Assert.Equal(987654321ul, parsed.ContentId);
        Assert.Equal(33u, parsed.HomeWorldId);
        Assert.Equal(33u, parsed.CurrentWorldId);
        Assert.False(parsed.IsOnline);
        Assert.Equal(0ul, parsed.OnlineStateMask);
        Assert.Equal(3, parsed.JobId);
        Assert.Equal(130u, parsed.LocationId);
    }

    [Fact]
    public void ParseEntry_ReturnsNull_WhenPointerIsNull() {
        var scanner = new MemoryFreeCompanyScanner();

        var parsed = scanner.ParseEntry(null);

        Assert.Null(parsed);
    }
}