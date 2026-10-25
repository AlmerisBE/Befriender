namespace Befriender.Core.GameData.Services;

using Befriender.Core.GameData.Contracts;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;
using System.Collections.Generic;
using System.Linq;

public class MainCommandProvider : IMainCommandProvider {
    private IDataManager dataManager;

    public MainCommandProvider(IDataManager dataManager) {
        this.dataManager = dataManager;
    }

    public IEnumerable<(uint Id, string Name, string Description)> GetAllMainCommands() {
        var sheet = this.dataManager.GetExcelSheet<MainCommand>();
        if (sheet == null) return Enumerable.Empty<(uint Id, string Name, string Description)>();

        // We explicitly name the tuple elements to allow property access like c.Id in LINQ expressions
        var commands = new List<(uint Id, string Name, string Description)>();

        foreach (var row in sheet) {
            string name = row.Name.ToString();

            if (row.RowId == 0 || string.IsNullOrWhiteSpace(name)) continue;

            string description = row.Description.ToString();
            commands.Add((row.RowId, name, description));
        }

        return commands.OrderBy(c => c.Id);
    }
}