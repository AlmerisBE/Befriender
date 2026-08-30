namespace Befriender.Core.FreeCompany.Contracts;

using Befriender.Core.FreeCompany.Models;
using System.Collections.Generic;

public interface IFreeCompanyRepository {
    void UpdateMembers(IEnumerable<FreeCompanyMemberProfile> scannedMembers);
}