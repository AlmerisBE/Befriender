namespace Befriender.UI.MainWindow.Lists.Consolidated;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Contracts;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.Theme.Contracts;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;

public class ConsolidatedTab : AbstractListTab {
    public override string InternalName => "Tab_Consolidated";
    public override string Name => this.loc.Translate("Tab_Consolidated");
    protected override string EmptyListMessageKey => "Consolidated_Empty";

    public ConsolidatedTab(
        ICharacterRegistry registry,
        ILocalizationService loc,
        IGameDataService gameDataService,
        IThemeService themeService,
        ITextureProvider textureProvider,
        IProximityService proximityService,
        ICharacterActionService actionService,
        ICharacterGroupRepository groupRepository,
        ICharacterTagRepository tagRepository,
        ListToolbarComponent toolbarComponent,
        CharacterProfilePanelComponent profilePanelComponent,
        IConfigurationService configurationService)
        : base(registry, loc, gameDataService, themeService, textureProvider, proximityService, actionService, groupRepository, tagRepository, toolbarComponent, profilePanelComponent, configurationService) {
    }

    protected override IEnumerable<Character> GetBaseCharacterList() {
        return this.registry.GetAllCharacters();
    }

    protected override IEnumerable<Character> SortCharacterList(IEnumerable<Character> characters) {
        return characters.OrderByDescending(c => c.IsActivelyTracked)
                         .ThenByDescending(c => c.IsOnline)
                         .ThenBy(c => c.Name);
    }
}