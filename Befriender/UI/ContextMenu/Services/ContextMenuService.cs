namespace Befriender.UI.ContextMenu.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Localization.Contracts;
using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using System;
using System.Linq;

public class ContextMenuService : IDisposable {
    private IContextMenu contextMenu;
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private IClientState clientState;
    private IObjectTable objectTable;

    public ContextMenuService(
        IContextMenu contextMenu,
        ICharacterRegistry registry,
        ILocalizationService loc,
        IClientState clientState,
        IObjectTable objectTable) {

        this.contextMenu = contextMenu;
        this.registry = registry;
        this.loc = loc;
        this.clientState = clientState;
        this.objectTable = objectTable;

        this.contextMenu.OnMenuOpened += this.OnMenuOpened;
    }

    private void OnMenuOpened(IMenuOpenedArgs args) {
        if (args.Target is not MenuTargetDefault target) return;

        // TargetName is already a string in Dalamud's ContextMenu API
        string name = target.TargetName;
        uint homeWorldId = target.TargetHomeWorld.RowId;

        if (string.IsNullOrWhiteSpace(name) || homeWorldId == 0) return;

        var existing = this.registry.GetAllCharacters().FirstOrDefault(c => c.IsSameIdentity(0, name, homeWorldId));
        if (existing != null && existing.IsActivelyTracked) {
            return;
        }

        args.AddMenuItem(new MenuItem {
            Name = this.loc.Translate("Action_AddToBefriender"),
            OnClicked = this.OnAddClicked
        });
    }

    private void OnAddClicked(IMenuItemClickedArgs args) {
        if (args.Target is not MenuTargetDefault target) return;

        // TargetName is already a string
        string name = target.TargetName;
        uint homeWorldId = target.TargetHomeWorld.RowId;
        ulong contentId = 0;

        if (target.TargetObjectId > 0) {
            var obj = this.objectTable.SearchById(target.TargetObjectId);
            if (obj is IPlayerCharacter pc) {
                unsafe {
                    var csChar = (Character*)pc.Address;
                    if (csChar != null) {
                        contentId = csChar->AccountId;
                    }
                }
            }
        }

        uint locationId = this.clientState.TerritoryType;

        this.registry.TryAddManualCharacter(contentId, name, homeWorldId, locationId);
    }

    public void Dispose() {
        this.contextMenu.OnMenuOpened -= this.OnMenuOpened;
    }
}