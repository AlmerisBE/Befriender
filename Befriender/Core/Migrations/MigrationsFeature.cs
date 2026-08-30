namespace Befriender.Core.Migrations;

using Befriender.Core.Framework;
using Befriender.Core.Migrations.Contracts;
using Befriender.Core.Migrations.Implementations;
using Befriender.Core.Migrations.Services;
using Microsoft.Extensions.DependencyInjection;

public class MigrationsFeature : IFeatureModule {
    public void RegisterServices(IServiceCollection services) {
        services.AddSingleton<IMigration, V1LegacyFriendStorageMigration>();
        services.AddSingleton<IMigrationService, MigrationService>();
    }
}