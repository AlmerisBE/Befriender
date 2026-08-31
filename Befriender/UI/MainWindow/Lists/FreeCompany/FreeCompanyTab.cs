namespace Befriender.UI.MainWindow.Lists.FreeCompany;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Contracts;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.Theme.Contracts;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class FreeCompanyTab : AbstractListTab {
    private Guid sourceId;

    public override string InternalName => "Tab_FreeCompany";
    public override string Name => this.loc.Translate("Tab_FreeCompany");
    protected override string EmptyListMessageKey => "FreeCompany_Empty";

    public FreeCompanyTab(ICharacterRegistry registry, IEnumerable<ICharacterSource> sources, ILocalizationService loc, IGameDataService gameDataService, IThemeService themeService, ITextureProvider textureProvider, IProximityService proximityService, ICharacterActionService actionService, ListToolbarComponent toolbarComponent, CharacterProfilePanelComponent profilePanelComponent)
        : base(registry, loc, gameDataService, themeService, textureProvider, proximityService, actionService, toolbarComponent, profilePanelComponent) {

        var src = sources.FirstOrDefault(s => s.Name == "FreeCompany");
        if (src != null) {
            this.sourceId = src.SourceId;
        }
    }

    protected override IEnumerable<Character> GetBaseCharacterList() {
        if (this.sourceId == Guid.Empty) {
            return Enumerable.Empty<Character>();
        }

        return this.registry.GetCharactersBySource(this.sourceId);
    }
}