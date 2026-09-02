namespace Befriender.UI.MainWindow.Lists.Others;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Contracts;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.MainWindow.Lists;
using Befriender.UI.Theme.Contracts;
using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;

public class OthersTab : AbstractListTab {
    public override string InternalName => "Tab_Others";
    public override string Name => this.loc.Translate("Tab_Others");
    public override int Order => 30;
    protected override string EmptyListMessageKey => "Others_Empty";

    public OthersTab(
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
        return this.registry.GetAllCharacters().Where(c => !c.IsActivelyTracked);
    }

    protected override IEnumerable<Character> SortCharacterList(IEnumerable<Character> characters) {
        return characters.OrderByDescending(c => c.IsOnline).ThenBy(c => c.Name);
    }
}