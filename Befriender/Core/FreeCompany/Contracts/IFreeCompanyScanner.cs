namespace Befriender.Core.FreeCompany.Contracts;

using Befriender.Core.FreeCompany.Models;
using System.Collections.Generic;

public interface IFreeCompanyScanner {
    IEnumerable<FreeCompanyMemberProfile> ScanMembers();
    int GetEntryCount();
    void RequestServerUpdate();
}