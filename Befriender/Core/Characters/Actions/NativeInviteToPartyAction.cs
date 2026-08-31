namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System;
using System.Text;

public unsafe class NativeInviteToPartyAction : ICharacterAction {
    private IObjectTable objectTable;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000011");
    public string InternalName => "Action_NativeInvite";
    public FontAwesomeIcon Icon => FontAwesomeIcon.UserPlus;

    public NativeInviteToPartyAction(IObjectTable objectTable) {
        this.objectTable = objectTable;
    }

    public bool CanExecute(Character character) {
        return !string.IsNullOrEmpty(character.Name);
    }

    public void Execute(Character character) {
        var inviteProxy = InfoProxyPartyInvite.Instance();
        if (inviteProxy == null) {
            return;
        }

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return;
        }

        var localWorldId = localPlayer.CurrentWorld.RowId;
        var targetWorldId = character.CurrentWorldId > 0 ? character.CurrentWorldId : character.HomeWorldId;

        if (targetWorldId == localWorldId) {
            var nameBytes = Encoding.UTF8.GetBytes(character.Name + "\0");
            fixed (byte* namePtr = nameBytes) {
                inviteProxy->InviteToParty(character.ContentId, namePtr, (ushort)character.HomeWorldId);
            }
        }
        else {
            inviteProxy->InviteToPartyContentId(character.ContentId, (ushort)targetWorldId);
        }
    }
}