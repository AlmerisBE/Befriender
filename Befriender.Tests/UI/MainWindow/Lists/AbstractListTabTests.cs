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
        protected override string EmptyListMessageKey => "Empty";

        public TestListTab(
            ICharacterRegistry registry, ILocalizationService loc, IGameDataService gameDataService,
            IThemeService themeService, ITextureProvider textureProvider, IProximityService proximityService,
            ICharacterActionService actionService, ICharacterGroupRepository groupRepository,
            ICharacterTagRepository tagRepository, ListToolbarComponent toolbarComponent,
            CharacterProfilePanelComponent profilePanelComponent, IConfigurationService configurationService)
            : base(registry, loc, gameDataService, themeService, textureProvider, proximityService, actionService, groupRepository, tagRepository, toolbarComponent, profilePanelComponent, configurationService) { }

        protected override IEnumerable<Character> GetBaseCharacterList() => new List<Character>();

        // Expose la propriété protégée pour pouvoir simuler un clic dans le test
        public void SelectCharacter(Character character) {
            this.selectedCharacter = character;
        }
    }

    [Fact]
    public void OnRegistryUpdated_ClearsSelection_WhenCharacterNoLongerInRegistry() {
        // Arrange
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockConfig = Substitute.For<IConfigurationService>();
        mockConfig.GetConfig().Returns(new PluginConfiguration());

        // On instancie la classe de test en utilisant "null!" pour les dépendances inutilisées dans ce contexte précis
        using var tab = new TestListTab(
            mockRegistry, null!, null!, null!, null!, null!, null!, null!, null!,
            new ListToolbarComponent(Substitute.For<ILocalizationService>()), null!, mockConfig);

        var charId = Guid.NewGuid();
        var character = new Character { Id = charId };

        tab.SelectCharacter(character);
        Assert.True(tab.IsProfilePanelOpen);

        // On simule le fait que le personnage a disparu du registre (ex: changement de personnage)
        mockRegistry.GetCharacterById(charId).Returns((Character)null!);

        // Act
        mockRegistry.RegistryUpdated += Raise.Event<Action>();

        // Assert
        Assert.False(tab.IsProfilePanelOpen);
    }
}