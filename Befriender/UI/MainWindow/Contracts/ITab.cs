namespace Befriender.UI.MainWindow.Contracts;

public interface ITab {
    string InternalName { get; }
    string Name { get; }
    bool IsProfilePanelOpen { get; }
    void Draw();
}