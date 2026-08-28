namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

public unsafe class ViewSearchInfoAction : IFriendAction {
    public string InternalName => "Action_ViewSearchInfo";
    public FontAwesomeIcon Icon => FontAwesomeIcon.InfoCircle;

    public bool CanExecute(FriendProfile friend) {
        return !friend.IsCharacterDeleted;
    }

    public void Execute(FriendProfile friend) {
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

        var count = friendProxy->InfoProxyPageInterface.InfoProxyInterface.GetEntryCount();
        for (uint i = 0; i < count; i++) {
            var entry = friendProxy->GetEntry(i);
            if (entry != null && entry->ContentId == friend.ContentId) {
                AgentDetail.Instance()->OpenForCharacterData(entry);
                break;
            }
        }
    }
}