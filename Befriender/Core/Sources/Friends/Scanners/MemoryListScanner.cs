namespace Befriender.Core.Sources.Friends.Scanners;

using Befriender.Core.Characters.Models;
using Befriender.Core.Sources.Friends.Contracts;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System;
using System.Collections.Generic;
using System.Text;

public unsafe class MemoryFriendListScanner : IFriendListScanner {
    public IEnumerable<Character> ScanActiveFriends() {
        var friends = new List<Character>();

        var uiModule = UIModule.Instance();
        if (uiModule == null) {
            return friends;
        }

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) {
            return friends;
        }

        var friendProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FriendList);
        if (friendProxy == null) {
            return friends;
        }

        var count = friendProxy->InfoProxyPageInterface.InfoProxyInterface.GetEntryCount();

        // Defensive safeguard against uninitialized memory causing infinite loops
        if (count > 200) {
            return friends;
        }

        for (uint i = 0; i < count; i++) {
            var entry = friendProxy->GetEntry(i);
            if (entry == null) {
                continue;
            }

            string name = string.Empty;
            var nameSpan = entry->Name;

            if (!nameSpan.IsEmpty) {
                int nullIndex = nameSpan.IndexOf((byte)0);
                if (nullIndex >= 0) {
                    nameSpan = nameSpan[..nullIndex];
                }

                name = Encoding.UTF8.GetString(nameSpan);
            }

            string fcTag = string.Empty;
            var fcTagSpan = entry->FCTag;

            if (!fcTagSpan.IsEmpty) {
                int nullIndex = fcTagSpan.IndexOf((byte)0);
                if (nullIndex >= 0) {
                    fcTagSpan = fcTagSpan[..nullIndex];
                }

                fcTag = Encoding.UTF8.GetString(fcTagSpan);
            }

            // Map directly to the Master List entity
            friends.Add(new Character {
                ContentId = entry->ContentId,
                Name = name,
                HomeWorldId = entry->HomeWorld,
                CurrentWorldId = entry->CurrentWorld,
                IsOnline = entry->State != 0,
                JobId = entry->Job,
                LocationId = entry->Location,
                FcTag = fcTag,
                OnlineStateMask = (ulong)entry->State,
                ClientLanguages = (byte)entry->Languages,
                GrandCompany = (byte)entry->GrandCompany
            });
        }

        return friends;
    }

    public int GetCurrentFriendCount() {
        var uiModule = UIModule.Instance();
        if (uiModule == null) {
            return 0;
        }

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) {
            return 0;
        }

        var friendProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FriendList);
        if (friendProxy == null) {
            return 0;
        }

        var count = friendProxy->InfoProxyPageInterface.InfoProxyInterface.GetEntryCount();
        if (count > 200) {
            return 0;
        }

        return (int)count;
    }

    public ulong GetStateHash() {
        var uiModule = UIModule.Instance();
        if (uiModule == null) {
            return 0;
        }

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) {
            return 0;
        }

        var friendProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FriendList);
        if (friendProxy == null) {
            return 0;
        }

        var count = friendProxy->InfoProxyPageInterface.InfoProxyInterface.GetEntryCount();
        if (count > 200) {
            return 0;
        }

        ulong hash = count;

        for (uint i = 0; i < count; i++) {
            var entry = friendProxy->GetEntry(i);
            if (entry != null) {
                hash = unchecked(hash * 314159 + (ulong)entry->State);
            }
        }

        return hash;
    }

    public void RequestServerUpdate() {
        var uiModule = UIModule.Instance();
        if (uiModule == null) {
            return;
        }

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) {
            return;
        }

        var friendProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FriendList);
        if (friendProxy == null) {
            return;
        }

        friendProxy->RequestData();
    }
}