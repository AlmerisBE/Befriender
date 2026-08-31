namespace Befriender.Core.Sources.FreeCompany.Contracts;

using Befriender.Core.Characters.Models;
using System.Collections.Generic;

public interface IFreeCompanyScanner {
    IEnumerable<Character> ScanMembers();
    int GetEntryCount();
    void RequestServerUpdate();
}