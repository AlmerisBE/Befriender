namespace Befriender.Tests.Core.Proximity.Services;

using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Interface.ImGuiNotification;
using Dalamud.Plugin.Services;
using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Configuration.Contracts;
using global::Befriender.Core.Configuration.Models;
using global::Befriender.Core.GameData.Contracts;
using global::Befriender.Core.Localization.Contracts;
using global::Befriender.Core.Proximity.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Xunit;

public class ProximityServiceTests {
    private ICharacterRegistry mockRegistry;
    private IObjectTable mockObjectTable;
    private IClientState mockClientState;
    private IConfigurationService mockConfig;
    private INotificationManager mockNotif;
    private ILocalizationService mockLoc;
    private IFramework mockFramework;
    private IGameDataService mockGameData;

    public ProximityServiceTests() {
        this.mockRegistry = Substitute.For<ICharacterRegistry>();
        this.mockObjectTable = Substitute.For<IObjectTable>();
        this.mockClientState = Substitute.For<IClientState>();
        this.mockConfig = Substitute.For<IConfigurationService>();
        this.mockNotif = Substitute.For<INotificationManager>();
        this.mockLoc = Substitute.For<ILocalizationService>();
        this.mockFramework = Substitute.For<IFramework>();
        this.mockGameData = Substitute.For<IGameDataService>();

        this.mockConfig.GetConfig().Returns(new PluginConfiguration { EnableProximityDetection = true });
    }

    private ProximityService CreateService() {
        return new ProximityService(
            this.mockRegistry, this.mockObjectTable, this.mockClientState, this.mockConfig,
            this.mockNotif, this.mockLoc, this.mockFramework, this.mockGameData);
    }

    // Helper method to bypass Lumina v4's read-only struct restrictions for testing RowRef<T>
    private Lumina.Excel.RowRef<Lumina.Excel.Sheets.World> CreateWorldRowRef(uint id) {
        object boxed = new Lumina.Excel.RowRef<Lumina.Excel.Sheets.World>();
        var type = typeof(Lumina.Excel.RowRef<Lumina.Excel.Sheets.World>);

        var fields = type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        bool set = false;
        foreach (var field in fields) {
            if (field.FieldType == typeof(uint) && field.Name.IndexOf("RowId", StringComparison.OrdinalIgnoreCase) >= 0) {
                field.SetValue(boxed, id);
                set = true;
                break;
            }
        }

        if (!set) {
            var prop = type.GetProperty("RowId", BindingFlags.Public | BindingFlags.Instance);
            if (prop != null && prop.CanWrite) {
                prop.SetValue(boxed, id);
            }
            else if (prop != null) {
                var backingField = type.GetField($"<{prop.Name}>k__BackingField", BindingFlags.NonPublic | BindingFlags.Instance);
                if (backingField != null) backingField.SetValue(boxed, id);
            }
        }

        return (Lumina.Excel.RowRef<Lumina.Excel.Sheets.World>)boxed;
    }

    private IPlayerCharacter CreateMockPlayer(IntPtr address, string name, uint worldId) {
        var mock = Substitute.For<IPlayerCharacter>();
        mock.Address.Returns(address);
        mock.Name.Returns((SeString)name);

        var worldRef = this.CreateWorldRowRef(worldId);
        mock.HomeWorld.Returns(worldRef);
        mock.CurrentWorld.Returns(worldRef);

        return mock;
    }

    [Fact]
    public void Constructor_SubscribesToFrameworkUpdate_AndDisposesCleanly() {
        var service = this.CreateService();
        this.mockFramework.Received(1).Update += Arg.Any<IFramework.OnUpdateDelegate>();

        service.Dispose();
        this.mockFramework.Received(1).Update -= Arg.Any<IFramework.OnUpdateDelegate>();
    }

    [Fact]
    public void OnFrameworkUpdate_ClearsNearbyIds_IfProximityDisabled() {
        this.mockConfig.GetConfig().Returns(new PluginConfiguration { EnableProximityDetection = false });
        using var service = this.CreateService();

        this.mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(this.mockFramework);

        var dummy = this.mockObjectTable.DidNotReceive().LocalPlayer;
    }

    [Fact]
    public void OnFrameworkUpdate_DetectsNewNearbyFriend_UpdatesStateAndNotifies() {
        var friendChar = new Character {
            Id = Guid.NewGuid(),
            ContentId = 12345,
            Name = "Alice",
            HomeWorldId = 33,
            ActiveSourceIds = new HashSet<Guid> { Guid.NewGuid() }
        };

        this.mockRegistry.GetAllCharacters().Returns(new List<Character> { friendChar });
        this.mockConfig.GetConfig().Returns(new PluginConfiguration { EnableProximityDetection = true, NotifyOnNearbyFriends = true });

        var mockLocal = this.CreateMockPlayer(new IntPtr(1), "LocalPlayer", 33);
        this.mockObjectTable.LocalPlayer.Returns(mockLocal);

        var mockFriendEntity = this.CreateMockPlayer(IntPtr.Zero, "Alice", 33);

        this.mockObjectTable.Length.Returns(2);
        this.mockObjectTable[0].Returns(mockLocal);
        this.mockObjectTable[1].Returns(mockFriendEntity);

        using var service = this.CreateService();

        this.mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(this.mockFramework);

        Assert.True(service.IsFriendNearby(friendChar.ContentId));
        this.mockRegistry.Received(1).SaveMasterList();
        this.mockNotif.Received(1).AddNotification(Arg.Is<Notification>(n => n.Title == "Befriender"));
    }

    [Fact]
    public void OnFrameworkUpdate_DetectsDepartedFriend_AndFiresEvent() {
        var friendId = Guid.NewGuid();
        var friendChar = new Character {
            Id = friendId,
            ContentId = 12345,
            Name = "Bob",
            HomeWorldId = 33,
            ActiveSourceIds = new HashSet<Guid> { Guid.NewGuid() }
        };

        this.mockRegistry.GetAllCharacters().Returns(new List<Character> { friendChar });
        this.mockRegistry.GetCharacterById(friendId).Returns(friendChar);

        var mockLocal = this.CreateMockPlayer(new IntPtr(1), "LocalPlayer", 33);
        this.mockObjectTable.LocalPlayer.Returns(mockLocal);

        var mockFriendEntity = this.CreateMockPlayer(IntPtr.Zero, "Bob", 33);

        this.mockObjectTable.Length.Returns(2);
        this.mockObjectTable[0].Returns(mockLocal);
        this.mockObjectTable[1].Returns(mockFriendEntity);

        using var service = this.CreateService();

        this.mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(this.mockFramework);
        Assert.True(service.IsFriendNearby(friendChar.ContentId));

        Thread.Sleep(2100);

        this.mockObjectTable.Length.Returns(1);

        bool eventFired = false;
        service.CharactersDeparted += chars => eventFired = true;

        this.mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(this.mockFramework);

        Assert.True(eventFired);
        Assert.False(service.IsFriendNearby(friendChar.ContentId));
    }

    [Fact]
    public void IsFriendNearby_ReturnsTrue_WhenCharacterIsTracked() {
        var friendChar = new Character {
            Id = Guid.NewGuid(),
            Name = "Charlie",
            HomeWorldId = 33,
            ContentId = 999,
            ActiveSourceIds = new HashSet<Guid> { Guid.NewGuid() }
        };
        this.mockRegistry.GetAllCharacters().Returns(new List<Character> { friendChar });

        var mockLocal = this.CreateMockPlayer(new IntPtr(1), "LocalPlayer", 33);
        this.mockObjectTable.LocalPlayer.Returns(mockLocal);

        var mockFriendEntity = this.CreateMockPlayer(IntPtr.Zero, "Charlie", 33);

        this.mockObjectTable.Length.Returns(2);
        this.mockObjectTable[0].Returns(mockLocal);
        this.mockObjectTable[1].Returns(mockFriendEntity);

        using var service = this.CreateService();
        this.mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(this.mockFramework);

        Assert.True(service.IsFriendNearby(999));
        Assert.Single(service.GetNearbyFriendIds());
        Assert.Equal(999ul, service.GetNearbyFriendIds()[0]);
    }
}