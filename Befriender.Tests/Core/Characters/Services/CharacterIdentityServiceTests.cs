namespace Befriender.Tests.Core.Characters.Services;

using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using global::Befriender.Core.Characters.Services;
using NSubstitute;
using Xunit;

public class CharacterIdentityServiceTests {
    [Fact]
    public void GetCurrentCharacterId_ReturnsEmpty_WhenNotLoggedIn() {
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();

        mockClientState.IsLoggedIn.Returns(false);

        var service = new CharacterIdentityService(mockClientState, mockObjectTable);

        Assert.Equal(string.Empty, service.GetCurrentCharacterId());
    }

    [Fact]
    public void GetCurrentCharacterId_ReturnsFormattedString_WhenLoggedInAndPlayerExists() {
        var mockClientState = Substitute.For<IClientState>();
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockPlayer = Substitute.For<IPlayerCharacter>();

        mockClientState.IsLoggedIn.Returns(true);
        mockPlayer.Name.Returns((SeString)"Ysaline Sylv'anir");

        // Lumina structures are read-only. NSubstitute will safely return default(World) where RowId = 0.
        // We adapt our test to expect 0 as the HomeWorldId.
        mockObjectTable.LocalPlayer.Returns(mockPlayer);

        var service = new CharacterIdentityService(mockClientState, mockObjectTable);

        var result = service.GetCurrentCharacterId();
        Assert.Equal("Ysaline Sylv'anir_0", result);
    }
}