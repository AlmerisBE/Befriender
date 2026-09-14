namespace Befriender.Tests.UI.MainWindow.Components;

using global::Befriender.Core.Characters.Contracts;
using global::Befriender.Core.Characters.Models;
using global::Befriender.Core.Localization.Contracts;
using global::Befriender.UI.MainWindow.Components;
using NSubstitute;
using System;
using System.Collections.Generic;
using Xunit;

public class GroupManagementComponentTests {
    private ICharacterGroupRepository mockGroupRepository;
    private ICharacterRegistry mockRegistry;
    private ILocalizationService mockLoc;

    public GroupManagementComponentTests() {
        this.mockGroupRepository = Substitute.For<ICharacterGroupRepository>();
        this.mockRegistry = Substitute.For<ICharacterRegistry>();
        this.mockLoc = Substitute.For<ILocalizationService>();
    }

    [Fact]
    public void CreateGroup_CallsRepository_WhenNameIsValid() {
        var component = new GroupManagementComponent(this.mockGroupRepository, this.mockRegistry, this.mockLoc);

        component.CreateGroup("New Group");

        this.mockGroupRepository.Received(1).AddGroup("New Group");
    }

    [Fact]
    public void DeleteGroup_UnassignsCharactersAndSaves_BeforeRemovingGroup() {
        var component = new GroupManagementComponent(this.mockGroupRepository, this.mockRegistry, this.mockLoc);
        var groupId = Guid.NewGuid();

        var char1 = new Character { Name = "Alice", CustomGroupId = groupId };
        var char2 = new Character { Name = "Bob", CustomGroupId = Guid.NewGuid() };
        this.mockRegistry.GetAllCharacters().Returns(new List<Character> { char1, char2 });

        component.DeleteGroup(groupId);

        Assert.Null(char1.CustomGroupId);
        Assert.NotNull(char2.CustomGroupId); // Unaffected character
        this.mockRegistry.Received(1).SaveMasterList();
        this.mockGroupRepository.Received(1).RemoveGroup(groupId);
    }
}