namespace Befriender.Core.Actions.Implementations;

using Befriender.Core.Actions.Contracts;
using Befriender.Core.Friends.Models;
using Dalamud.Interface;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using Lumina.Excel.Sheets;
using System;

public unsafe class JoinFriendAction : IFriendAction {
    private IObjectTable objectTable;
    private IDataManager dataManager;
    private IPluginLog pluginLog;

    public string InternalName => "Action_JoinFriend";
    public FontAwesomeIcon Icon => FontAwesomeIcon.MapMarkedAlt;

    public JoinFriendAction(IObjectTable objectTable, IDataManager dataManager, IPluginLog pluginLog) {
        this.objectTable = objectTable;
        this.dataManager = dataManager;
        this.pluginLog = pluginLog;
    }

    public bool CanExecute(FriendProfile friend) {
        if (!friend.IsOnline || friend.IsCharacterDeleted || friend.LocationId == 0) {
            return false;
        }

        var localPlayer = this.objectTable.LocalPlayer;
        if (localPlayer == null) {
            return false;
        }

        // The player must be on the exact same world to teleport to their territory
        var friendWorldId = friend.CurrentWorldId > 0 ? friend.CurrentWorldId : friend.HomeWorldId;
        if (localPlayer.CurrentWorld.RowId != friendWorldId) {
            return false;
        }

        var territorySheet = this.dataManager.GetExcelSheet<TerritoryType>();
        if (territorySheet == null) {
            return false;
        }

        var territory = territorySheet.GetRowOrDefault(friend.LocationId);
        if (!territory.HasValue) {
            return false;
        }

        // The action is only available if the territory possesses a valid aetheryte
        return territory.Value.Aetheryte.RowId > 0;
    }

    public void Execute(FriendProfile friend) {
        var territorySheet = this.dataManager.GetExcelSheet<TerritoryType>();
        if (territorySheet == null) {
            return;
        }

        var territory = territorySheet.GetRowOrDefault(friend.LocationId);
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
                this.pluginLog.Error(ex, "Failed to execute native teleportation to friend's aetheryte.");
            }
        }
    }
}