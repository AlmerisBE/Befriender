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
    public void StartSync_TriggersRequestServerRefreshWhenPreviouslyInactive() {
        var mockScanner = Substitute.For<IFreeCompanyScanner>();
        var mockRepo = Substitute.For<IFreeCompanyRepository>();
        var mockFramework = Substitute.For<IFramework>();

        using var service = new FreeCompanySyncService(mockScanner, mockRepo, mockFramework);

        service.StartSync();

        mockScanner.Received(1).RequestServerUpdate();
    }

    [Fact]
    public void OnFrameworkUpdate_StreamsPartialDataInstantlyAndDelaysFinalization() {
        var mockScanner = Substitute.For<IFreeCompanyScanner>();
        var mockRepo = Substitute.For<IFreeCompanyRepository>();
        var mockFramework = Substitute.For<IFramework>();

        var fakeMembers = new List<FreeCompanyMemberProfile> {
            new FreeCompanyMemberProfile { ContentId = 1, Name = "Test Member" }
        };

        mockScanner.GetEntryCount().Returns(5);
        mockScanner.ScanMembers().Returns(fakeMembers);

        using var service = new FreeCompanySyncService(mockScanner, mockRepo, mockFramework);

        service.StartSync(); // Triggers the request

        // Trigger framework update - it detects 5 members (chunk arrived)
        mockFramework.Update += Raise.Event<IFramework.OnUpdateDelegate>(mockFramework);

        // Assert it streamed the partial sync immediately (isFinalSync = false)
        mockRepo.Received(1).UpdateMembers(Arg.Any<IEnumerable<FreeCompanyMemberProfile>>(), false);

        // Assert it did NOT finalize yet (isFinalSync = true)
        mockRepo.DidNotReceive().UpdateMembers(Arg.Any<IEnumerable<FreeCompanyMemberProfile>>(), true);
    }
}