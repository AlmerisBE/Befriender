namespace Befriender.UI.MainWindow.Tabs;

using Befriender.Core.Localization.Contracts;
using Befriender.UI.MainWindow.Contracts;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Components;
using System.Diagnostics;
using System.Numerics;
using System.Reflection;

public class AboutTab : ITab {
    private ILocalizationService loc;
    private string pluginVersion;

    public string InternalName => "Tab_About";
    public string Name => this.loc.Translate("Tab_About");
    public int Order => 50;
    public bool IsProfilePanelOpen => false;

    public AboutTab(ILocalizationService loc) {
        this.loc = loc;
        this.pluginVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "Unknown";
    }

    public void Draw() {
        float footerHeight = ImGui.GetFrameHeight() + (ImGui.GetStyle().ItemSpacing.Y * 3);

        if (ImGui.BeginChild("AboutContent", new Vector2(0, -footerHeight))) {
            ImGui.TextWrapped(this.loc.Translate("About_Description"));

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextDisabled(this.loc.Translate("About_FeaturesHeader"));
            ImGui.Spacing();

            ImGui.Bullet();
            ImGui.TextWrapped(this.loc.Translate("About_Feature1"));

            ImGui.Bullet();
            ImGui.TextWrapped(this.loc.Translate("About_Feature2"));

            ImGui.Bullet();
            ImGui.TextWrapped(this.loc.Translate("About_Feature3"));

            ImGui.Bullet();
            ImGui.TextWrapped(this.loc.Translate("About_Feature4"));

            ImGui.Bullet();
            ImGui.TextWrapped(this.loc.Translate("About_Feature5"));

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.Text($"{this.loc.Translate("About_Version")}: {this.pluginVersion}");

            ImGui.Spacing();

            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.CodeBranch, this.loc.Translate("About_GitHub"))) {
                try {
                    Process.Start(new ProcessStartInfo {
                        FileName = "https://github.com/AlmerisBE/Befriender",
                        UseShellExecute = true
                    });
                }
                catch { } // Silently ignore if the OS fails to open the browser
            }

            ImGui.SameLine();

            if (ImGuiComponents.IconButtonWithText(FontAwesomeIcon.Comments, this.loc.Translate("About_Discord"))) {
                try {
                    Process.Start(new ProcessStartInfo {
                        FileName = "https://discord.gg/3VKgxb3Sy",
                        UseShellExecute = true
                    });
                }
                catch { } // Silently ignore if the OS fails to open the browser
            }
        }
        ImGui.EndChild();
    }
}