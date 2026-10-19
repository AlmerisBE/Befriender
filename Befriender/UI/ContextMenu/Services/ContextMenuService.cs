namespace Befriender.UI.ContextMenu.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Localization.Contracts;
using Dalamud.Game.Gui.ContextMenu;
using Dalamud.Plugin.Services;
using System;
using System.Linq;

public class ContextMenuService : IDisposable {
    private IContextMenu contextMenu;
    private ICharacterRegistry registry;
    private ILocalizationService loc;
    private IClientState clientState;

    public ContextMenuService(
        IContextMenu contextMenu,
        ICharacterRegistry registry,
        ILocalizationService loc,
        IClientState clientState) {

        this.contextMenu = contextMenu;
        this.registry = registry;
        this.loc = loc;
        this.clientState = clientState;

        this.contextMenu.OnMenuOpened += this.OnMenuOpened;
    }

    private void OnMenuOpened(IMenuOpenedArgs args) {
        if (args.Target is not MenuTargetDefault target) return;

        string name = target.TargetName;
        uint homeWorldId = target.TargetHomeWorld.RowId;

        if (string.IsNullOrWhiteSpace(name) || homeWorldId == 0) return;

        var existing = this.registry.GetAllCharacters().FirstOrDefault(c =>
            string.Equals(c.Name, name, StringComparison.Ordinal) && c.HomeWorldId == homeWorldId);

        if (existing == null || !existing.IsActivelyTracked) {
            args.AddMenuItem(new MenuItem {
                Name = this.loc.Translate("Action_AddToBefriender"),
                OnClicked = (a) => this.OnAddClicked(target)
            });
        }
    }

    private void OnAddClicked(MenuTargetDefault target) {
        string name = target.TargetName;
        uint homeWorldId = target.TargetHomeWorld.RowId;
        uint locationId = this.clientState.TerritoryType;

        // We pass 0 for contentId because the native FFXIV client does not securely broadcast
        // the ContentId of nearby untracked players (reading memory here yields garbage values).
        // The registry will seamlessly fallback to identifying the contact by Name + HomeWorldId.
        this.registry.TryAddManualCharacter(0, name, homeWorldId, locationId);
    }

    public void Dispose() {
        this.contextMenu.OnMenuOpened -= this.OnMenuOpened;
    }
}