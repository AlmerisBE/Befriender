namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Agent;
using System;

public unsafe class ViewAdventurerPlateAction : ICharacterAction {
    private IPluginLog pluginLog;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000023");
    public string InternalName => "Action_ViewAdventurerPlate";
    public FontAwesomeIcon Icon => FontAwesomeIcon.AddressCard;

    public ViewAdventurerPlateAction(IPluginLog pluginLog) {
        this.pluginLog = pluginLog;
    }

    public bool CanExecute(Character character) {
        return !string.IsNullOrEmpty(character.Name);
    }

    public void Execute(Character character) {
        try {
            var agent = AgentCharaCard.Instance();
            if (agent != null) {
                agent->OpenCharaCard(character.ContentId);
            }
        }
        catch (Exception ex) {
            this.pluginLog.Error(ex, "Unable to open adventurer plate natively.");
        }
    }
}