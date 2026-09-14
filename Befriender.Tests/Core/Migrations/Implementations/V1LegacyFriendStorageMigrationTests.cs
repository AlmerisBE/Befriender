namespace Befriender.Tests.Core.Migrations.Implementations;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.Characters.Models;
using Befriender.Core.Migrations.Implementations;
using Dalamud.Plugin;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

public class V1LegacyFriendStorageMigrationTests : IDisposable {
    private IDalamudPluginInterface mockPluginInterface;
    private ICharacterStorage mockStorage;
    private string tempDirectory;
    private string accountIdentity = "TestAccount_123";

    public V1LegacyFriendStorageMigrationTests() {
        this.mockPluginInterface = Substitute.For<IDalamudPluginInterface>();
        this.mockStorage = Substitute.For<ICharacterStorage>();

        this.tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(this.tempDirectory);

        this.mockPluginInterface.ConfigDirectory.Returns(new DirectoryInfo(this.tempDirectory));
    }

    [Fact]
    public void Execute_DoesNothing_WhenLegacyFileDoesNotExist() {
        var migration = new V1LegacyFriendStorageMigration(this.mockPluginInterface, this.mockStorage);

        migration.Execute(this.accountIdentity);

        this.mockStorage.DidNotReceiveWithAnyArgs().Save(default!, default!, default!);
    }

    [Fact]
    public void Execute_MigratesDataAndRenamesFile_WhenLegacyFileIsValid() {
        var migration = new V1LegacyFriendStorageMigration(this.mockPluginInterface, this.mockStorage);
        string filePath = Path.Combine(this.tempDirectory, $"friends_{this.accountIdentity}.json");

        // Simulating the legacy camelCase JSON structure
        string legacyJson = @"[
            {
                ""Id"": ""12345678-1234-1234-1234-123456789012"",
                ""ContentId"": 999,
                ""Name"": ""Legacy Friend"",
                ""HomeWorldId"": 33,
                ""IsArchived"": false,
                ""IsMissing"": false
            }
        ]";
        File.WriteAllText(filePath, legacyJson);

        migration.Execute(this.accountIdentity);

        // Verify that the new storage engine was called with the correctly mapped data
        this.mockStorage.Received(1).Save(
            "MasterCharacterList",
            this.accountIdentity,
            Arg.Is<IEnumerable<Character>>(chars =>
                chars.Count() == 1 &&
                chars.First().Name == "Legacy Friend" &&
                chars.First().ContentId == 999 &&
                chars.First().ActiveSourceIds.Contains(Guid.Parse("51000000-0000-0000-0000-000000000001"))
            )
        );

        // Verify that the legacy file was renamed to avoid duplicate migrations
        Assert.False(File.Exists(filePath));
        Assert.True(File.Exists($"{filePath}.migrated"));
    }

    [Fact]
    public void Execute_FailsSafely_WhenFileIsCorrupted() {
        var migration = new V1LegacyFriendStorageMigration(this.mockPluginInterface, this.mockStorage);
        string filePath = Path.Combine(this.tempDirectory, $"friends_{this.accountIdentity}.json");

        File.WriteAllText(filePath, "{ Not a valid JSON array }");

        var exception = Record.Exception(() => migration.Execute(this.accountIdentity));

        Assert.Null(exception);
        this.mockStorage.DidNotReceiveWithAnyArgs().Save(default!, default!, default!);
        Assert.True(File.Exists(filePath)); // File shouldn't be renamed if it failed
    }

    public void Dispose() {
        if (Directory.Exists(this.tempDirectory)) Directory.Delete(this.tempDirectory, true);
    }
}