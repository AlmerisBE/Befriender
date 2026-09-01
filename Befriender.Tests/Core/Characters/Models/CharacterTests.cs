namespace Befriender.Tests.Core.Characters.Models;

using global::Befriender.Core.Characters.Models;
using Xunit;

public class CharacterTests {
    [Fact]
    public void IsSameIdentity_ReturnsTrue_WhenContentIdMatches() {
        var character = new Character { ContentId = 12345, Name = "Alice", HomeWorldId = 33 };

        // Even if name and world are different, a matching non-zero ContentId is the absolute truth
        bool result = character.IsSameIdentity(12345, "Bob", 44);

        Assert.True(result);
    }

    [Fact]
    public void IsSameIdentity_ReturnsTrue_WhenNameAndHomeWorldMatch() {
        var character = new Character { ContentId = 0, Name = "Alice", HomeWorldId = 33 };

        bool result = character.IsSameIdentity(0, "Alice", 33);

        Assert.True(result);
    }

    [Fact]
    public void IsSameIdentity_ReturnsFalse_WhenNothingMatches() {
        var character = new Character { ContentId = 12345, Name = "Alice", HomeWorldId = 33 };

        bool result = character.IsSameIdentity(99999, "Bob", 44);

        Assert.False(result);
    }
}