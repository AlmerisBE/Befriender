namespace Befriender.Core.FreeCompany.Scanners;

using Befriender.Core.FreeCompany.Contracts;
using Befriender.Core.FreeCompany.Models;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System;
using System.Collections.Generic;
using System.Text;

public unsafe class MemoryFreeCompanyScanner : IFreeCompanyScanner {
    public IEnumerable<FreeCompanyMemberProfile> ScanMembers() {
        var members = new List<FreeCompanyMemberProfile>();
        var uiModule = UIModule.Instance();
        if (uiModule == null) {
            return members;
        }

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) {
            return members;
        }

        // FIX: Use FreeCompanyMember instead of FreeCompany to get the actual roster list
        var fcProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FreeCompanyMember);
        if (fcProxy == null) {
            return members;
        }

        var count = fcProxy->InfoProxyPageInterface.InfoProxyInterface.GetEntryCount();

        // Defensive safeguard: An FC can have at most 512 members. 
        // If it's higher, memory is likely uninitialized or corrupted.
        if (count > 1000) {
            return members;
        }

        for (uint i = 0; i < count; i++) {
            var entry = fcProxy->GetEntry(i);
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

            members.Add(new FreeCompanyMemberProfile {
                ContentId = entry->ContentId,
                Name = name,
                HomeWorldId = entry->HomeWorld,
                CurrentWorldId = entry->CurrentWorld,
                JobId = entry->Job,
                LocationId = entry->Location,
                IsOnline = entry->State != 0,
                OnlineStateMask = (ulong)entry->State,
                FcTag = fcTag
            });
        }

        return members;
    }

    public int GetEntryCount() {
        var uiModule = UIModule.Instance();
        if (uiModule == null) {
            return 0;
        }

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) {
            return 0;
        }

        // FIX: Use FreeCompanyMember here as well
        var fcProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FreeCompanyMember);
        if (fcProxy == null) {
            return 0;
        }

        var count = fcProxy->InfoProxyPageInterface.InfoProxyInterface.GetEntryCount();

        if (count > 1000) {
            return 0;
        }

        return (int)count;
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

        // FIX: Use FreeCompanyMember to request the roster data
        var fcProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FreeCompanyMember);
        if (fcProxy == null) {
            return;
        }

        fcProxy->RequestData();
    }
}