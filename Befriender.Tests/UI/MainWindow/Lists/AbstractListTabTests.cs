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
        public override int Order => 0;
        protected override string EmptyListMessageKey => "Empty";

        public TestListTab(
            ICharacterRegistry registry, ILocalizationService loc, IGameDataService gameDataService,
            IThemeService themeService, ITextureProvider textureProvider, IProximityService proximityService,
            ICharacterActionService actionService, ICharacterGroupRepository groupRepository,
            ICharacterTagRepository tagRepository, ListToolbarComponent toolbarComponent,
            CharacterProfilePanelComponent profilePanelComponent, IConfigurationService configurationService, IGameGui gameGui, ICharacterSearchService characterSearchService)
            : base(registry, loc, gameDataService, themeService, textureProvider, proximityService, actionService, groupRepository, tagRepository, toolbarComponent, profilePanelComponent, configurationService, gameGui, characterSearchService) { }

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
            new ListToolbarComponent(Substitute.For<ILocalizationService>()), null!, mockConfig, null!, null!);

        var charId = Guid.NewGuid();
        var character = new Character { Id = charId };

        tab.SelectCharacter(character);
        Assert.True(tab.IsProfilePanelOpen);

        mockRegistry.GetCharacterById(charId).Returns((Character)null!);

        mockRegistry.RegistryUpdated += Raise.Event<Action>();

        Assert.False(tab.IsProfilePanelOpen);
    }

    [Fact]
    public void Constructor_LoadsActiveColumns_FromTabStateConfiguration() {
        // Arrange
        var mockRegistry = Substitute.For<ICharacterRegistry>();
        var mockConfigService = Substitute.For<IConfigurationService>();
        var config = new PluginConfiguration();

        config.TabStates["TestTab"] = new TabState {
            ActiveColumns = new List<ColumnType> { ColumnType.Name, ColumnType.Level }
        };

        mockConfigService.GetConfig().Returns(config);

        // Act
        using var tab = new TestListTab(
            mockRegistry, null!, null!, null!, null!, null!, null!, null!, null!,
            new ListToolbarComponent(Substitute.For<ILocalizationService>()), null!, mockConfigService, null!, null!);

        // Assert
        var field = typeof(AbstractListTab).GetField("activeColumns", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var loadedColumns = field?.GetValue(tab) as List<ColumnType>;

        Assert.NotNull(loadedColumns);
        Assert.Equal(2, loadedColumns.Count);
        Assert.Contains(ColumnType.Name, loadedColumns);
        Assert.Contains(ColumnType.Level, loadedColumns);
    }
}