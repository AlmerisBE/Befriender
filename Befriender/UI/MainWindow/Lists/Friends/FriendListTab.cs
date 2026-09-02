namespace Befriender.UI.MainWindow.Lists.Friends;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Contracts;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.Theme.Contracts;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class FriendListTab : AbstractListTab {
    private Guid sourceId;

    public override string InternalName => "Tab_List";
    public override string Name => this.loc.Translate("Tab_List");
    public override int Order => 10;
    protected override string EmptyListMessageKey => "List_Empty";

    public FriendListTab(
        ICharacterRegistry registry,
        IEnumerable<ICharacterSource> sources,
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

        var src = sources.FirstOrDefault(s => s.Name == "FriendList");
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