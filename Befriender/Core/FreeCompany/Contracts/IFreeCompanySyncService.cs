namespace Befriender.Core.FreeCompany.Contracts;

public interface IFreeCompanySyncService {
    void StartSync();
    void StopSync();
    void ForceSync(bool isFinalSync = true);
    void RequestServerRefresh();
}