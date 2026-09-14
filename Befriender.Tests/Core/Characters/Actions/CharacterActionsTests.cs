namespace Befriender.Tests.Core.Characters.Actions;

using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using global::Befriender.Core.Characters.Actions;
using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.GameData.Contracts;
using NSubstitute;
using System;
using Xunit;

public class CharacterActionsTests {
    // --- DeleteCharacterDataAction ---
    [Fact]
    public void DeleteCharacterDataAction_CanExecute_OnlyIfCharacterIsNotActivelyTracked() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var action = new DeleteCharacterDataAction(mockRegistry);

        var activelyTrackedChar = new Character();
        activelyTrackedChar.ActiveSourceIds.Add(Guid.NewGuid());

        var untrackedChar = new Character();

        Assert.False(action.CanExecute(activelyTrackedChar));
        Assert.True(action.CanExecute(untrackedChar));
    }

    [Fact]
    public void DeleteCharacterDataAction_Execute_RemovesFromRegistryAndSaves() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var action = new DeleteCharacterDataAction(mockRegistry);
        var targetChar = new Character { Id = Guid.NewGuid() };

        action.Execute(targetChar);

        mockRegistry.Received(1).RemoveCharacter(targetChar.Id);
        mockRegistry.Received(1).SaveMasterList();
    }

    // --- Track / Untrack Actions ---
    [Fact]
    public void TrackCharacterAction_CanExecute_WhenValidAndNotAlreadyTracked() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var action = new TrackCharacterAction(mockRegistry);

        var validChar = new Character { Name = "John Doe" };
        validChar.ActiveSourceIds.Add(Guid.NewGuid());
        validChar.IsTrackedForNotifications = false;

        Assert.True(action.CanExecute(validChar));

        validChar.IsTrackedForNotifications = true;
        Assert.False(action.CanExecute(validChar));
    }

    [Fact]
    public void TrackCharacterAction_Execute_EnablesTrackingAndSaves() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var action = new TrackCharacterAction(mockRegistry);
        var targetChar = new Character { IsTrackedForNotifications = false };

        action.Execute(targetChar);

        Assert.True(targetChar.IsTrackedForNotifications);
        mockRegistry.Received(1).SaveMasterList();
    }

    [Fact]
    public void UntrackCharacterAction_Execute_DisablesTrackingAndSaves() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var action = new UntrackCharacterAction(mockRegistry);
        var targetChar = new Character { IsTrackedForNotifications = true };

        action.Execute(targetChar);

        Assert.False(targetChar.IsTrackedForNotifications);
        mockRegistry.Received(1).SaveMasterList();
    }

    // --- Removal Mark / Unmark Actions ---
    [Fact]
    public void RequestRemoveCharacterAction_Execute_InvokesService() {
        var mockRequestService = Substitute.For<IRemoveCharacterRequestService>();
        var action = new RequestRemoveCharacterAction(mockRequestService);
        var targetChar = new Character();

        action.Execute(targetChar);

        mockRequestService.Received(1).RequestRemoval(targetChar);
    }

    [Fact]
    public void UnmarkForRemovalAction_Execute_ClearsMarkAndSaves() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var action = new UnmarkForRemovalAction(mockRegistry);
        var targetChar = new Character { IsMarkedForRemoval = true };

        action.Execute(targetChar);

        Assert.False(targetChar.IsMarkedForRemoval);
        mockRegistry.Received(1).SaveMasterList();
    }

    // --- CopyNameAction ---
    [Fact]
    public void CopyNameAction_CanExecute_OnlyIfNameIsNotEmpty() {
        var mockChatGui = Substitute.For<IChatGui>();
        var action = new CopyNameAction(mockChatGui);

        var validChar = new Character { Name = "John Doe" };
        var invalidChar = new Character { Name = "" };

        Assert.True(action.CanExecute(validChar));
        Assert.False(action.CanExecute(invalidChar));
    }

    // --- ViewPartyFinderListingAction ---
    [Fact]
    public void ViewPartyFinderListingAction_CanExecute_RequiresRecruitingStatus() {
        var action = new ViewPartyFinderListingAction();

        var notOnlineChar = new Character { Name = "John", IsOnline = false };
        Assert.False(action.CanExecute(notOnlineChar));

        var onlineNotRecruiting = new Character {
            Name = "John",
            IsOnline = true,
            OnlineStateMask = (ulong)InfoProxyCommonList.CharacterData.OnlineStatus.Busy
        };
        Assert.False(action.CanExecute(onlineNotRecruiting));

        var onlineRecruiting = new Character {
            Name = "John",
            IsOnline = true,
            OnlineStateMask = (ulong)InfoProxyCommonList.CharacterData.OnlineStatus.RecruitingPartyMembers
        };
        Assert.True(action.CanExecute(onlineRecruiting));
    }

    // --- JoinCharacterAction ---
    [Fact]
    public void JoinCharacterAction_CanExecute_ReturnsFalse_WhenOfflineOrNoLocation() {
        var mockObjectTable = Substitute.For<IObjectTable>();
        var mockDataManager = Substitute.For<IDataManager>();
        var mockPluginLog = Substitute.For<IPluginLog>();
        var action = new JoinCharacterAction(mockObjectTable, mockDataManager, mockPluginLog);

        Assert.False(action.CanExecute(new Character { IsOnline = false, Name = "Alice", LocationId = 123 }));
        Assert.False(action.CanExecute(new Character { IsOnline = true, Name = "", LocationId = 123 }));
        Assert.False(action.CanExecute(new Character { IsOnline = true, Name = "Alice", LocationId = 0 }));
    }

    // --- SendTellAction ---
    [Fact]
    public void SendTellAction_CanExecute_OnlyIfNameIsNotEmpty() {
        var mockGameDataService = Substitute.For<IGameDataService>();
        var action = new SendTellAction(mockGameDataService);

        Assert.True(action.CanExecute(new Character { Name = "Alice" }));
        Assert.False(action.CanExecute(new Character { Name = "" }));
    }

    // --- NativeInviteToPartyAction ---
    [Fact]
    public void NativeInviteToPartyAction_CanExecute_OnlyIfNameIsNotEmpty() {
        var mockObjectTable = Substitute.For<IObjectTable>();
        var action = new NativeInviteToPartyAction(mockObjectTable);

        Assert.True(action.CanExecute(new Character { Name = "Alice" }));
        Assert.False(action.CanExecute(new Character { Name = "" }));
    }

    // --- EstateTeleportationAction ---
    [Fact]
    public void EstateTeleportationAction_CanExecute_RequiresActiveTrackingAndName() {
        var action = new EstateTeleportationAction();

        var valid = new Character { Name = "Alice" };
        valid.ActiveSourceIds.Add(Guid.NewGuid());

        var untracked = new Character { Name = "Bob" };

        var noName = new Character { Name = "" };
        noName.ActiveSourceIds.Add(Guid.NewGuid());

        Assert.True(action.CanExecute(valid));
        Assert.False(action.CanExecute(untracked));
        Assert.False(action.CanExecute(noName));
    }

    // --- ViewAdventurerPlateAction & ViewSearchInfoAction ---
    [Fact]
    public void InfoActions_CanExecute_OnlyIfNameIsNotEmpty() {
        var mockPluginLog = Substitute.For<IPluginLog>();
        var plateAction = new ViewAdventurerPlateAction(mockPluginLog);
        var searchAction = new ViewSearchInfoAction();

        var validChar = new Character { Name = "Alice" };
        var invalidChar = new Character { Name = "" };

        Assert.True(plateAction.CanExecute(validChar));
        Assert.False(plateAction.CanExecute(invalidChar));

        Assert.True(searchAction.CanExecute(validChar));
        Assert.False(searchAction.CanExecute(invalidChar));
    }
}