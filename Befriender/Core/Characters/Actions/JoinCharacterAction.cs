namespace Befriender.Core.Characters.Actions;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System;

public unsafe class JoinCharacterAction : ICharacterAction {
    private IObjectTable objectTable;
    private IDataManager dataManager;
    private IPluginLog pluginLog;

    public Guid ActionId { get; } = Guid.Parse("A1000000-0000-0000-0000-000000000010");
    public string InternalName => "Action_JoinFriend";
    public FontAwesomeIcon Icon => FontAwesomeIcon.MapMarkedAlt;

    public JoinCharacterAction(IObjectTable objectTable, IDataManager dataManager, IPluginLog pluginLog) {
        this.objectTable = objectTable;
        this.dataManager = dataManager;
        this.pluginLog = pluginLog;
    }

    public bool CanExecute(Character character) {
        if (!character.IsOnline || string.IsNullOrEmpty(character.Name) || character.LocationId == 0) {
            return false;
        }

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return false;
        }

        var targetWorldId = character.CurrentWorldId > 0 ? character.CurrentWorldId : character.HomeWorldId;
        if (localPlayer.CurrentWorld.RowId != targetWorldId) {
            return false;
        }

        var territorySheet = this.dataManager.GetExcelSheet<TerritoryType>();
        if (territorySheet == null) {
            return false;
        }

        var territory = territorySheet.GetRowOrDefault(character.LocationId);
        if (!territory.HasValue) {
            return false;
        }

        return territory.Value.Aetheryte.RowId > 0;
    }

    public void Execute(Character character) {
        var territorySheet = this.dataManager.GetExcelSheet<TerritoryType>();
        if (territorySheet == null) {
            return;
        }

        var territory = territorySheet.GetRowOrDefault(character.LocationId);
        if (!territory.HasValue) {
            return;
        }

        var aetheryteId = territory.Value.Aetheryte.RowId;
        if (aetheryteId > 0) {
            try {
                var telepo = Telepo.Instance();
                if (telepo != null) {
                    telepo->Teleport(aetheryteId, 0);
                }
            }
            catch (Exception ex) {
                this.pluginLog.Error(ex, "Failed to execute native teleportation to character's aetheryte.");
            }
        }
    }
}