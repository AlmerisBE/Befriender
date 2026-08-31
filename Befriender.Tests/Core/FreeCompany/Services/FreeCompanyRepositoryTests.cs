namespace Befriender.Tests.Core.FreeCompany.Services;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.FreeCompany.Models;
using Befriender.Core.FreeCompany.Services;
using Befriender.Core.Friends.Contracts;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using Xunit;

public class FreeCompanyRepositoryTests {
    [Fact]
    public void UpdateMembers_AddsNewMembersAndUpdatesExistingOnes() {
        var mockStorage = Substitute.For<ICharacterStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var existingMember = new Character { ContentId = 1, Name = "Old Name", IsOnline = false };
        mockStorage.Load("FreeCompanyList", "Almeris_33").Returns(new List<Character> { existingMember });

        var repository = new FreeCompanyRepository(mockStorage, mockIdentityService);

        var scannedMembers = new List<FreeCompanyMemberProfile> {
            new FreeCompanyMemberProfile { ContentId = 1, Name = "New Name", IsOnline = true, JobId = 20 },
            new FreeCompanyMemberProfile { ContentId = 2, Name = "Brand New Member", IsOnline = false }
        };

        repository.UpdateMembers(scannedMembers);
        var result = repository.GetCharacters().ToList();

        Assert.Equal(2, result.Count);

        var updatedMember = result.First(m => m.ContentId == 1);
        Assert.Equal("New Name", updatedMember.Name);
        Assert.True(updatedMember.IsOnline);
        Assert.Equal(20, updatedMember.JobId);

        var addedMember = result.First(m => m.ContentId == 2);
        Assert.Equal("Brand New Member", addedMember.Name);
        Assert.False(addedMember.IsOnline);

        mockStorage.Received(1).Save("FreeCompanyList", "Almeris_33", Arg.Is<IEnumerable<Character>>(chars => chars.Count() == 2));
    }

    [Fact]
    public void UpdateMembers_RemovesMembersNoLongerInFreeCompany_OnlyOnFinalSync() {
        var mockStorage = Substitute.For<ICharacterStorage>();
        var mockIdentityService = Substitute.For<ICharacterIdentityService>();

        mockIdentityService.GetCurrentCharacterId().Returns("Almeris_33");

        var memberToKeep = new Character { ContentId = 1, Name = "Keep Me" };
        var memberToRemove = new Character { ContentId = 2, Name = "Remove Me" };
        mockStorage.Load("FreeCompanyList", "Almeris_33").Returns(new List<Character> { memberToKeep, memberToRemove });

        var repository = new FreeCompanyRepository(mockStorage, mockIdentityService);

        var scannedMembers = new List<FreeCompanyMemberProfile> {
            new FreeCompanyMemberProfile { ContentId = 1, Name = "Keep Me", IsOnline = true }
        };

        // Act - Partial Sync (Should NOT remove members)
        repository.UpdateMembers(scannedMembers, false);
        var resultPartial = repository.GetCharacters().ToList();
        Assert.Equal(2, resultPartial.Count); // Member 2 is still safe!

        // Act - Final Sync (Should clean up)
        repository.UpdateMembers(scannedMembers, true);
        var resultFinal = repository.GetCharacters().ToList();
        Assert.Single(resultFinal);
        Assert.Equal(1ul, resultFinal[0].ContentId);
    }
}