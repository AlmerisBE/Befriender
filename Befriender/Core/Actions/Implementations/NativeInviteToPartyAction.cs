namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System.Text;

public unsafe class NativeInviteToPartyAction : IFriendAction {
    private IObjectTable objectTable;

    public string InternalName => "Action_NativeInvite";
    public FontAwesomeIcon Icon => FontAwesomeIcon.UserPlus;

    public NativeInviteToPartyAction(IObjectTable objectTable) {
        this.objectTable = objectTable;
    }

    public bool CanExecute(FriendProfile friend) {
        return !friend.IsCharacterDeleted;
    }

    public void Execute(FriendProfile friend) {
        var inviteProxy = InfoProxyPartyInvite.Instance();
        if (inviteProxy == null) {
            return;
        }

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return;
        }

        var localWorldId = localPlayer.CurrentWorld.RowId;
        var friendWorldId = friend.CurrentWorldId > 0 ? friend.CurrentWorldId : friend.HomeWorldId;

        if (friendWorldId == localWorldId) {
            var nameBytes = Encoding.UTF8.GetBytes(friend.Name + "\0");
            fixed (byte* namePtr = nameBytes) {
                inviteProxy->InviteToParty(friend.ContentId, namePtr, (ushort)friend.HomeWorldId);
            }
        }
        else {
            inviteProxy->InviteToPartyContentId(friend.ContentId, (ushort)friendWorldId);
        }
    }
}