namespace Befriender.Core.GameData.Contracts;

using System.Collections.Generic;

public interface IMainCommandProvider {
    IEnumerable<(uint Id, string Name, string Description)> GetAllMainCommands();
}