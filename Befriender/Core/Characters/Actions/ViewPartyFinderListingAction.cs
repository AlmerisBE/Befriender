namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using FFXIVClientStructs.FFXIV.Client.UI.Info;
using System;

public unsafe class ViewPartyFinderListingAction : ICharacterAction {
    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000024");
    public string InternalName => "Action_ViewPartyFinder";
    public FontAwesomeIcon Icon => FontAwesomeIcon.UsersViewfinder;

    public bool CanExecute(Character character) {
        if (string.IsNullOrEmpty(character.Name) || !character.IsOnline) {
            return false;
        }

        var state = (InfoProxyCommonList.CharacterData.OnlineStatus)character.OnlineStateMask;
        return state.HasFlag(InfoProxyCommonList.CharacterData.OnlineStatus.RecruitingPartyMembers);
    }

    public void Execute(Character character) {
        var agent = AgentLookingForGroup.Instance();
        if (agent != null) {
            agent->OpenListingByContentId(character.ContentId);
        }
    }
}