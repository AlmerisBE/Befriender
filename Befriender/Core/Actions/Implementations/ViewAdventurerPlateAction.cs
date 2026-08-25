namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;

public unsafe class ViewAdventurerPlateAction : IFriendAction {
    private IPluginLog pluginLog;

    public string InternalName => "Action_ViewAdventurerPlate";
    public FontAwesomeIcon Icon => FontAwesomeIcon.AddressCard;

    public ViewAdventurerPlateAction(IPluginLog pluginLog) {
        this.pluginLog = pluginLog;
    }

    public bool CanExecute(FriendProfile friend) {
        return !friend.IsCharacterDeleted;
    }

    public void Execute(FriendProfile friend) {
        try {
            var agent = AgentCharaCard.Instance();
            if (agent != null) {
                agent->OpenCharaCard(friend.ContentId);
            }
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, "Unable to open adventurer plate natively.");
        }
    }
}