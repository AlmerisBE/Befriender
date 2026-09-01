namespace Befriender.Tests.Core.Migrations.Services;

using Dalamud.Plugin.Services;
using global::Befriender.Core.Migrations.Contracts;
using global::Befriender.Core.Migrations.Services;
using NSubstitute;
using System;
using Xunit;

public class MigrationServiceTests {
    [Fact]
    public void RunMigrations_ExecutesInOrderOfTargetVersion() {
        var mockLog = Substitute.For<IPluginLog>();

        var mig1 = Substitute.For<IMigration>();
        mig1.TargetVersion.Returns(1);

        var mig2 = Substitute.For<IMigration>();
        mig2.TargetVersion.Returns(2);

        // Pass them in wrong order to test sorting
        var service = new MigrationService(new[] { mig2, mig1 }, mockLog);

        service.RunMigrations("TestAccount_33");

        Received.InOrder(() => {
            mig1.Execute("TestAccount_33");
            mig2.Execute("TestAccount_33");
        });
    }

    [Fact]
    public void RunMigrations_StopsExecution_IfOneMigrationFails() {
        var mockLog = Substitute.For<IPluginLog>();

        var mig1 = Substitute.For<IMigration>();
        mig1.TargetVersion.Returns(1);
        mig1.When(x => x.Execute(Arg.Any<string>())).Do(x => throw new Exception("Migration failed"));

        var mig2 = Substitute.For<IMigration>();
        mig2.TargetVersion.Returns(2);

        var service = new MigrationService(new[] { mig1, mig2 }, mockLog);

        service.RunMigrations("TestAccount_33");

        mig1.Received(1).Execute("TestAccount_33");
        mig2.DidNotReceive().Execute(Arg.Any<string>()); // Should not execute due to previous failure
        mockLog.Received(1).Error(Arg.Any<Exception>(), Arg.Is<string>(s => s.Contains("failed for account TestAccount_33")));
    }

    [Fact]
    public void RunMigrations_DoesNotRunTwice_ForSameAccount() {
        var mockLog = Substitute.For<IPluginLog>();
        var mig1 = Substitute.For<IMigration>();
        mig1.TargetVersion.Returns(1);

        var service = new MigrationService(new[] { mig1 }, mockLog);

        service.RunMigrations("TestAccount_33");
        service.RunMigrations("TestAccount_33"); // Second call

        mig1.Received(1).Execute("TestAccount_33"); // Still only 1 call
    }
}