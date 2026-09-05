namespace Befriender.Core.Sources.FreeCompany.Scanners;

using Befriender.Core.Characters.Models;
using Befriender.Core.Sources.FreeCompany.Contracts;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System;
using System.Collections.Generic;
using System.Text;

public unsafe class MemoryFreeCompanyScanner : IFreeCompanyScanner {
    public IEnumerable<Character> ScanMembers() {
        var members = new List<Character>();
        var uiModule = UIModule.Instance();
        if (uiModule == null) return members;

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) return members;

        var fcProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FreeCompanyMember);
        if (fcProxy == null) return members;

        var count = fcProxy->InfoProxyPageInterface.InfoProxyInterface.GetEntryCount();
        if (count > 1000) return members; // Sanity check

        for (uint i = 0; i < count; i++) {
            var entry = fcProxy->GetEntry(i);
            if (entry == null) continue;

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

            members.Add(new Character {
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
        if (uiModule == null) return 0;

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) return 0;

        var fcProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FreeCompanyMember);
        if (fcProxy == null) return 0;

        var count = fcProxy->InfoProxyPageInterface.InfoProxyInterface.GetEntryCount();
        if (count > 1000) return 0;

        return (int)count;
    }

    public void RequestServerUpdate() {
        var uiModule = UIModule.Instance();
        if (uiModule == null) return;

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) return;

        // FFXIV Server bandwidth optimization: 
        // The server ignores FreeCompanyMember list requests if the main Free Company profile is not initialized.
        // Opening the native UI triggers both. We replicate this native execution flow here.
        var fcProfileProxy = infoModule->GetInfoProxyById(InfoProxyId.FreeCompany);
        if (fcProfileProxy != null) {
            fcProfileProxy->RequestData();
        }

        var fcMemberProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FreeCompanyMember);
        if (fcMemberProxy != null) {
            fcMemberProxy->RequestData();
        }
    }

    public ulong GetStateHash() {
        var uiModule = UIModule.Instance();
        if (uiModule == null) return 0;

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) return 0;

        var fcProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FreeCompanyMember);
        if (fcProxy == null) return 0;

        var count = fcProxy->InfoProxyPageInterface.InfoProxyInterface.GetEntryCount();
        if (count > 1000) return 0;

        ulong hash = count;

        for (uint i = 0; i < count; i++) {
            var entry = fcProxy->GetEntry(i);
            if (entry != null) {
                hash = unchecked(hash * 314159 + (ulong)entry->State);
            }
        }

        return hash;
    }
}