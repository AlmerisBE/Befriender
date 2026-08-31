namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System;

public unsafe class ViewSearchInfoAction : ICharacterAction {
    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000025");
    public string InternalName => "Action_ViewSearchInfo";
    public FontAwesomeIcon Icon => FontAwesomeIcon.InfoCircle;

    public bool CanExecute(Character character) {
        return !string.IsNullOrEmpty(character.Name);
    }

    public void Execute(Character character) {
        var uiModule = UIModule.Instance();
        if (uiModule == null) {
            return;
        }

        var infoModule = uiModule->GetInfoModule();
        if (infoModule == null) {
            return;
        }

        // Native search info usually requires the target to be in the FriendList proxy
        var friendProxy = (InfoProxyCommonList*)infoModule->GetInfoProxyById(InfoProxyId.FriendList);
        if (friendProxy == null) {
            return;
        }

        var count = friendProxy->InfoProxyPageInterface.InfoProxyInterface.GetEntryCount();
        for (uint i = 0; i < count; i++) {
            var entry = friendProxy->GetEntry(i);
            if (entry != null && entry->ContentId == character.ContentId) {
                AgentDetail.Instance()->OpenForCharacterData(entry);
                break;
            }
        }
    }
}