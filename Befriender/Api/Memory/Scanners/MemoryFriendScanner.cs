namespace Befriender.Api.Memory.Scanners;

using Befriender.Core.Friends.Contracts;
using Befriender.Core.Friends.Models;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System;
using System.Collections.Generic;
using System.Text;

public unsafe class MemoryFriendScanner : IFriendScanner {
    public IEnumerable<FriendProfile> ScanActiveFriends() {
        var friends = new List<FriendProfile>();

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

            if (string.IsNullOrWhiteSpace(name)) {
                continue;
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

            bool isOnline = entry->State != 0;

            friends.Add(new FriendProfile {
                ContentId = entry->ContentId,
                Name = name,
                HomeWorldId = entry->HomeWorld,
                IsOnline = isOnline,
                JobId = entry->Job,
                LocationId = entry->Location,
                FcTag = fcTag
            });
        }

        return friends;
    }
}