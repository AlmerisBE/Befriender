namespace Befriender.Core.FreeCompany.Contracts;

using Befriender.Core.FreeCompany.Models;
using System;
using System.Collections.Generic;

public interface IFreeCompanyRepository {
    Guid SourceId { get; }
    void UpdateMembers(IEnumerable<FreeCompanyMemberProfile> scannedMembers);
}