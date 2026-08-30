namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Characters.Contracts;
using Befriender.Core.FreeCompany.Contracts;
using Befriender.Core.GameData.Contracts;
using Befriender.Core.Localization.Contracts;
using Befriender.UI.Theme.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using System.Linq;

public class FreeCompanyTab : ITab {
    private ICharacterRegistry registry;
    private IFreeCompanyRepository fcRepository;
    private ILocalizationService loc;
    private IGameDataService gameDataService;
    private IThemeService themeService;

    public string InternalName => "Tab_FreeCompany";
    public string Name => this.loc.Translate("Tab_FreeCompany");
    public bool IsProfilePanelOpen => false;

    public FreeCompanyTab(ICharacterRegistry registry, IFreeCompanyRepository fcRepository, ILocalizationService loc, IGameDataService gameDataService, IThemeService themeService) {
        this.registry = registry;
        this.fcRepository = fcRepository;
        this.loc = loc;
        this.gameDataService = gameDataService;
        this.themeService = themeService;
    }

    public void Draw() {
        var fcSourceId = this.fcRepository.SourceId;
        var palette = this.themeService.CurrentPalette;

        // Fetch characters tracked by the Free Company source
        var members = this.registry.GetConsolidatedCharacters()
            .Where(c => c.ActiveSourceIds.Contains(fcSourceId))
            .OrderByDescending(c => c.IsOnline)
            .ThenBy(c => c.Name)
            .ToList();

        if (members.Count == 0) {
            ImGui.TextDisabled(this.loc.Translate("FreeCompany_Empty"));
            return;
        }

        if (ImGui.BeginTable("FcMembersTable", 4, ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.Resizable | ImGuiTableFlags.ScrollY)) {
            ImGui.TableSetupColumn(this.loc.Translate("Column_Name"));
            ImGui.TableSetupColumn(this.loc.Translate("Column_Status"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Column_Job"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(this.loc.Translate("Section_Notes"));
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableHeadersRow();

            foreach (var member in members) {
                ImGui.TableNextRow();

                // --- Name ---
                ImGui.TableNextColumn();
                ImGui.Text(member.Name);

                // --- Status ---
                ImGui.TableNextColumn();
                if (member.IsOnline) {
                    ImGui.TextColored(palette.TextOnline, this.loc.Translate("Profile_Online"));
                }
                else {
                    ImGui.TextColored(palette.TextOffline, this.loc.Translate("Status_Offline"));
                }

                // --- Level / Job ---
                ImGui.TableNextColumn();
                if (member.JobId > 0) {
                    var jobAbbr = this.gameDataService.GetJobAbbreviation(member.JobId);
                    ImGui.Text(member.Level > 0 ? $"Lv {member.Level} {jobAbbr}" : jobAbbr);
                }
                else {
                    ImGui.Text(this.loc.Translate("Profile_Unknown"));
                }

                // --- Notes ---
                ImGui.TableNextColumn();
                ImGui.Text(member.Notes);
            }

            ImGui.EndTable();
        }
    }
}