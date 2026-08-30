namespace Befriender.Core.Migrations.Services;

using Befriender.Core.Migrations.Contracts;
using Dalamud.Plugin.Services;
using System;
using System.Collections.Generic;
using System.Linq;

public class MigrationService : IMigrationService {
    private IEnumerable<IMigration> migrations;
    private IPluginLog pluginLog;
    private HashSet<string> migratedAccounts = new();

    public MigrationService(IEnumerable<IMigration> migrations, IPluginLog pluginLog) {
        this.migrations = migrations;
        this.pluginLog = pluginLog;
    }

    public void RunMigrations(string accountIdentity) {
        if (string.IsNullOrEmpty(accountIdentity) || this.migratedAccounts.Contains(accountIdentity)) {
            return;
        }

        var orderedMigrations = this.migrations.OrderBy(m => m.TargetVersion).ToList();

        foreach (var migration in orderedMigrations) {
            try {
                migration.Execute(accountIdentity);
            }
            catch (Exception ex) {
                this.pluginLog.Error(ex, $"Migration V{migration.TargetVersion} failed for account {accountIdentity}");
                return; // Stop execution to prevent data corruption down the line
            }
        }

        this.migratedAccounts.Add(accountIdentity);
    }
}