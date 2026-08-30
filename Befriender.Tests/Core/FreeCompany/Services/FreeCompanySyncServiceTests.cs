namespace Befriender.Tests.Core.FreeCompany.Services;

using Befriender.Core.FreeCompany.Contracts;
using Befriender.Core.FreeCompany.Models;
using Befriender.Core.FreeCompany.Services;
using Dalamud.Plugin.Services;
using NSubstitute;
using System.Collections.Generic;
using Xunit;

public class FreeCompanySyncServiceTests {
    [Fact]
    public void StartSync_TriggersImmediateSyncWhenPreviouslyInactive() {
        var mockScanner = Substitute.For<IFreeCompanyScanner>();
        var mockRepo = Substitute.For<IFreeCompanyRepository>();
        var mockFramework = Substitute.For<IFramework>();

        mockScanner.ScanMembers().Returns(new List<FreeCompanyMemberProfile>());

        using var service = new FreeCompanySyncService(mockScanner, mockRepo, mockFramework);

        service.StartSync();

        mockScanner.Received(1).ScanMembers();
        mockRepo.Received(1).UpdateMembers(Arg.Any<IEnumerable<FreeCompanyMemberProfile>>());
    }

    [Fact]
    public void ForceSync_DelegatesScannedMembersToRepository() {
        var mockScanner = Substitute.For<IFreeCompanyScanner>();
        var mockRepo = Substitute.For<IFreeCompanyRepository>();
        var mockFramework = Substitute.For<IFramework>();

        var fakeMembers = new List<FreeCompanyMemberProfile> {
            new FreeCompanyMemberProfile { ContentId = 1, Name = "Test Member" }
        };
        mockScanner.ScanMembers().Returns(fakeMembers);

        using var service = new FreeCompanySyncService(mockScanner, mockRepo, mockFramework);

        service.ForceSync();

        mockRepo.Received(1).UpdateMembers(fakeMembers);
    }
}