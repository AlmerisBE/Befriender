namespace Befriender.UI.FriendList.Tabs;

using Befriender.Core.Localization.Contracts;
using Befriender.UI.Windows.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using System.Diagnostics;
using System.Reflection;

public class AboutTab : ITab {
    private ILocalizationService loc;
    private string pluginVersion;

    public string InternalName => "Tab_About";
    public string Name => this.loc.Translate("Tab_About");
    public bool IsProfilePanelOpen => false;

    public AboutTab(ILocalizationService loc) {
        this.loc = loc;
        this.pluginVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    }

    public void Draw() {
        ImGui.TextWrapped(this.loc.Translate("About_Description"));

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextDisabled(this.loc.Translate("About_FeaturesHeader"));
        ImGui.Spacing();
        ImGui.BulletText(this.loc.Translate("About_Feature1"));
        ImGui.BulletText(this.loc.Translate("About_Feature2"));
        ImGui.BulletText(this.loc.Translate("About_Feature3"));
        ImGui.BulletText(this.loc.Translate("About_Feature4"));
        ImGui.BulletText(this.loc.Translate("About_Feature5"));

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.Text($"{this.loc.Translate("About_Version")}: {this.pluginVersion}");

        ImGui.Spacing();

        // Using CodeBranch as a safe fallback for the repository link
        if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.CodeBranch, this.loc.Translate("About_GitHub"))) {
            try {
                Process.Start(new ProcessStartInfo {
                    FileName = "https://github.com/AlmerisBE/Befriender",
                    UseShellExecute = true
                });
            }
            catch { } // Silently ignore if the OS fails to open the browser
        }
    }
}