namespace Befriender.Tests.UI.MainWindow.Lists;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Configuration.Contracts;
using Befriender.Core.Configuration.Models;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.Core.Proximity.Contracts;
using Befriender.UI.MainWindow.Components;
using Befriender.UI.MainWindow.Lists;
using Befriender.UI.Theme.Contracts;
using Dalamud.Plugin.Services;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class AbstractListTabTests {
    private class TestListTab : AbstractListTab {
        public override string InternalName => "TestTab";
        public override string Name => "Test";
        public override int Order => 0; // Added the missing abstract member implementation
        protected override string EmptyListMessageKey => "Empty";

        public TestListTab(
            ICharacterRegistry registry, ILocalizationService loc, IGameDataService gameDataService,
            IThemeService themeService, ITextureProvider textureProvider, IProximityService proximityService,
            ICharacterActionService actionService, ICharacterGroupRepository groupRepository,
            ICharacterTagRepository tagRepository, ListToolbarComponent toolbarComponent,
            CharacterProfilePanelComponent profilePanelComponent, IConfigurationService configurationService)
            : base(registry, loc, gameDataService, themeService, textureProvider, proximityService, actionService, groupRepository, tagRepository, toolbarComponent, profilePanelComponent, configurationService) { }

        protected override IEnumerable<Character> GetBaseCharacterList() => new List<Character>();

        public void SelectCharacter(Character character) {
            this.selectedCharacter = character;
        }
    }

    [Fact]
    public void OnRegistryUpdated_ClearsSelection_WhenCharacterNoLongerInRegistry() {
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockConfig = Substitute.For<IConfigurationService>();
        mockConfig.GetConfig().Returns(new PluginConfiguration());

        using var tab = new TestListTab(
            mockRegistry, null!, null!, null!, null!, null!, null!, null!, null!,
            new ListToolbarComponent(Substitute.For<ILocalizationService>()), null!, mockConfig);

        var charId = Guid.NewGuid();
        var character = new Character { Id = charId };

        tab.SelectCharacter(character);
        Assert.True(tab.IsProfilePanelOpen);

        mockRegistry.GetCharacterById(charId).Returns((Character)null!);

        mockRegistry.RegistryUpdated += Raise.Event<Action>();

        Assert.False(tab.IsProfilePanelOpen);
    }
}