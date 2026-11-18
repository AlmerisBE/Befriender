namespace Befriender.Tests.Core.Characters.Actions;

using Befriender.Core.Characters.Actions;
using Befriender.Core.Characters.Models;
using Befriender.Core.GameData.Contracts;
using NSubstitute;
using Xunit;

public class SendTellActionTests {
    [Fact]
    public void Execute_RequestsHomeWorldName_AndIgnoresCurrentWorld() {
        // Arrange
        var mockGameDataService = Substitute.For<IGameDataService>();
        var mockChatBoxService = Substitute.For<IChatBoxService>();

        var action = new SendTellAction(mockGameDataService, mockChatBoxService);

        var character = new Character {
            Name = "John Doe",
            HomeWorldId = 42,
            CurrentWorldId = 99
        };

        mockGameDataService.GetWorldName(42).Returns("Cerberus");

        // Act
        action.Execute(character);

        // Assert
        mockGameDataService.Received(1).GetWorldName(42);
        mockGameDataService.DidNotReceive().GetWorldName(99);

        // Bonus TDD : Verify the exact command string passed to the native abstraction!
        mockChatBoxService.Received(1).SendMessage("/tell John Doe@Cerberus ");
    }
}