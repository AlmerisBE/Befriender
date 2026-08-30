namespace Befriender.Core.Migrations.Contracts;

public interface IMigration {
    int TargetVersion { get; }
    void Execute(string accountIdentity);
}