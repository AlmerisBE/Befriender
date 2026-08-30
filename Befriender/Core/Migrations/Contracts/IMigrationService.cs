namespace Befriender.Core.Migrations.Contracts;

public interface IMigrationService {
    void RunMigrations(string accountIdentity);
}